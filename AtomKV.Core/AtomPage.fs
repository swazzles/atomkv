namespace AtomKV.Core

open System
open System.IO
open System.Security.Cryptography
open AtomKV.Core.Types
open AtomKV.Core.AtomFile
open AtomKV.Core.AtomIndex

type AtomPage = {
    Name: string
    PageFile: AtomFile
    IndexFile: AtomFile
}


module AtomPage = 

    let private getPageFileName (pageName:string) = $"{pageName}.page"

    let private getPageIndexFileName (pageName:string) = $"{pageName}.page.index"

    let private getDocumentHash (doc:byte[]) = 
        use hasher = SHA1.Create()
        let hashBytes = hasher.ComputeHash(doc)
        Convert.ToHexString(hashBytes)

    let private getIndex (page:AtomPage) (key:string) =        
        AtomFile.search page.IndexFile (fun x -> x.StartsWith key)
            |> function
                | location, Some(line) -> Some(AtomIndex.indexFromString location line)
                | _, None -> None

    let private getDocument atom index page = 
        AtomFile.readBytes atom page.PageFile index.DocumentStart index.DocumentLength

    let private createDocument atom page key doc hash = 
        let documentStart = AtomFile.appendBytes atom page.PageFile doc
        let index = {
            IndexLocation = page.IndexFile.Writer.BaseStream.Position
            Key = key
            Hash = hash
            DocumentStart = documentStart
            DocumentLength = doc.Length
        }   
        AtomIndex.indexToString index
            |> AtomFile.appendLine page.IndexFile


    let private updateDocument atom page index doc newHash = 
        let documentStart = AtomFile.appendBytes atom page.PageFile doc
        let newIndex = {
            index with
                Hash = newHash
                DocumentStart = documentStart
                DocumentLength = doc.Length
        }

        let indexRecord = indexToString newIndex

        AtomFile.writeLineAt page.IndexFile newIndex.IndexLocation indexRecord
          
    let openPage (pageName:string) = 
        {
            Name = pageName
            PageFile = getPageFileName pageName |> openFile
            IndexFile = getPageIndexFileName pageName |> openFile
        }
    
    let closePage (page:AtomPage) = 
        closeAtomFile page.PageFile
        closeAtomFile page.IndexFile

    let deletePage (page:AtomPage) =          
        closePage page
        page.Name |> getPageFileName |> deleteFile
        page.Name |> getPageIndexFileName |> deleteFile      
       
    let commitPage page =
        AtomFile.commit page.PageFile
        AtomFile.commit page.IndexFile

    let put atom (page:AtomPage) (key:string) (doc:byte[]) =               
        let index = getIndex page key
        let hash = getDocumentHash doc
        let requiresUpdate = AtomIndex.requiresUpdate index hash

        if not requiresUpdate then
           {
                Status = PutResponseStatus.NoUpdate
           }
        else
            match index with
            | Some(x) -> updateDocument atom page x doc hash
            | None -> createDocument atom page key doc hash
            
            commitPage page

            {
                Status = PutResponseStatus.Ok
            }

    let get atom (page:AtomPage) (key:string) = 
        match getIndex page key with
        | Some(index) -> {
                Document = Some(getDocument atom index page)
                Status = GetResponseStatus.Ok
            }
        | None -> { 
            Document = None 
            Status = GetResponseStatus.KeyNotExist 
            }
        
        
     

        

