namespace AtomKV.Core

open System
open System.IO
open System.Security.Cryptography
open AtomKV.Core.Types
open AtomKV.Core.AtomFile
open AtomKV.Core.AtomIndex
open System.Threading.Channels
open System.Threading.Tasks

type AtomPage = {
    Name: string
    PageFile: AtomFile
    IndexFile: AtomFile
    RequestChannel: Channel<AtomRequestPromise>
}

(*
    Handles reading and writing documents to a page file on disk.
    A page is equivalent to one shard in the local keyspace.
    NOTE: Do not run a single page 
*)
module AtomPage = 
    open System.Threading

    let private getPageFileName (pageName:string) = $"{pageName}.page"

    let private getPageIndexFileName (pageName:string) = $"{pageName}.page.index"    

    let private getDocumentHash (doc:byte[]) = 
        use hasher = SHA1.Create()
        let hashBytes = hasher.ComputeHash(doc)
        Convert.ToHexString(hashBytes)

    let private getIndex (page:AtomPage) (key:string) =      
        let search = AtomFile.search page.IndexFile (fun x -> x.StartsWith key)
        match search with
                | location, Some line -> 
                    let indexData = AtomIndex.indexFromString line
                    Some {
                        IndexLocation = location
                        Data = indexData
                    }
                | _, None -> None

    let private getDocument atom index page = 
        AtomFile.readBytes atom page.PageFile index.Data.DocumentStart index.Data.DocumentLength

    let private createDocument atom page key doc hash = 
        let (documentStart, documentLength) = AtomFile.appendBytes atom page.PageFile doc
        let index = {
            Key = key
            Hash = hash
            DocumentStart = documentStart
            DocumentLength = documentLength
        }   
        let indexString = AtomIndex.indexToString index
        AtomFile.appendLine page.IndexFile indexString |> ignore


    let private updateDocument atom page (index:AtomIndex) doc newHash = 
        let (documentStart, documentLength) = AtomFile.appendBytes atom page.PageFile doc
        let newIndex = {
            index with
                Data = {
                    index.Data with
                        Hash = newHash
                        DocumentStart = documentStart
                        DocumentLength = documentLength
                }
        }

        let indexRecord = indexToString newIndex.Data

        AtomFile.writeLineAt page.IndexFile newIndex.IndexLocation indexRecord

    let private processPut atom (page:AtomPage) (req: PutRequest) =    
        let index = getIndex page req.Key
        let hash = getDocumentHash req.Doc
        let requiresUpdate = AtomIndex.requiresUpdate index hash

        if not requiresUpdate then
            {
                RequestId = req.RequestId
                Status = PutResponseStatus.NoUpdate
            }
        else
            match index with
            | Some(x) -> updateDocument atom page x req.Doc hash
            | None -> createDocument atom page req.Key req.Doc hash

            {
                RequestId = req.RequestId
                Status = PutResponseStatus.Ok
            }

    let private processGet atom (page:AtomPage) (req:GetRequest) = 
        match getIndex page req.Key with
        | Some index -> 
            let document = getDocument atom index page
            {
                RequestId = req.RequestId
                Document = Some document
                Status = GetResponseStatus.Ok
            }
        | None -> { 
            RequestId = req.RequestId
            Document = None 
            Status = GetResponseStatus.KeyNotExist 
        }
          
    let getPage (pageName:string) =          
        let channelOptions = BoundedChannelOptions(AtomConstants.pageChannelBounding)
        channelOptions.SingleReader <- true
        channelOptions.SingleWriter <- false
        channelOptions.FullMode <- BoundedChannelFullMode.DropWrite
        
        {
            Name = pageName
            PageFile = getPageFileName pageName |> AtomFile.openFile
            IndexFile = getPageIndexFileName pageName |> AtomFile.openFile
            RequestChannel = Channel.CreateBounded<AtomRequestPromise>(channelOptions)
        }

    let closePage page =
        async {
            AtomFile.closeFile page.PageFile
            AtomFile.closeFile page.IndexFile
        }

    let deletePage (page:AtomPage) =
        async {
            closeFile page.PageFile
            closeFile page.IndexFile
            page.Name |> getPageFileName |> deleteFile
            page.Name |> getPageIndexFileName |> deleteFile            
        }

    let processNextRequest atom page =
        async {            
            let! next = page.RequestChannel.Reader.ReadAsync().AsTask() |> Async.AwaitTask
            let res = 
                match next.Request with
                | PutRequest(t)-> PutResponse(processPut atom page t)
                | GetRequest(t) -> GetResponse(processGet atom page t) 

            do! next.CompletionChannel.Writer.WriteAsync(res).AsTask() |> Async.AwaitTask
        }

    let processRequestsAsync atom page (cancellationToken:CancellationToken) = 
        async {
            while not cancellationToken.IsCancellationRequested do
                do! processNextRequest atom page
        }
     

        

