namespace AtomKV.Core

open System
open System.IO
open SimpleBase
open System.Security.Cryptography
open System.Collections.Generic


type AtomFile = {
    Reader: StreamReader
    Writer: StreamWriter
}

type AtomIndex = {
    Exists:bool
    Key:string
    Hash:string
    DocumentLocation:int64
}

type AtomPage = {
    Name: string
    PageFile: AtomFile
    IndexFile: AtomFile
    Index: Dictionary<string, AtomIndex>
    Page: List<byte[]>
}


module AtomPage = 


    (*
        Open a file with handlers for reading and writing.
    *)
    let openFile (fileName:string) =
        let fileName = fileName
        let file = File.Open(fileName, FileMode.OpenOrCreate)
        let reader = new StreamReader(file)
        let writer = new StreamWriter(file)        
        {
            Reader = reader
            Writer = writer
        }

    let closeFile (file:Stream) =
        match file.CanRead || file.CanWrite with
            | true -> file.Close()
            | false -> ()

    let closeAtomFile (file:AtomFile) =
        closeFile file.Reader.BaseStream
        closeFile file.Writer.BaseStream


    let deleteFile (fileName:string) =
        match File.Exists fileName with
            |true -> File.Delete fileName
            |false -> ()

    let getPageFileName (pageName:string) = 
        String.Format("{0}.page", pageName)

    let getPageIndexFileName (pageName:string) = 
        String.Format("{0}.page.index", pageName)

    let createIndexFromString (index:string) = 
        if index = null then
            {
                Exists = false
                Key = null
                Hash = null
                DocumentLocation = 0L
            }
        else
            {
                Exists = true
                Key = index.Substring(0, AtomConstants.keyLength).TrimEnd(AtomConstants.indexPadCharacter)
                Hash = index.Substring(AtomConstants.keyLength, AtomConstants.indexHashLength)
                DocumentLocation = int64(index.Substring(AtomConstants.keyLength + AtomConstants.indexHashLength, AtomConstants.indexLength).TrimEnd(AtomConstants.indexPadCharacter))
            }
    
    let openPage (pageName:string) = 
        let page = {
            Name = pageName
            PageFile = getPageFileName pageName |> openFile
            IndexFile = getPageIndexFileName pageName |> openFile
            Index = Dictionary<string, AtomIndex>()
            Page = List<byte[]>()
        }

        while page.IndexFile.Reader.Peek() > -1 do
            let line = page.IndexFile.Reader.ReadLine()
            let index = createIndexFromString line
            page.Index.Add(index.Key, index)

        while page.PageFile.Reader.Peek() > -1 do
            let line = page.PageFile.Reader.ReadLine()
            page.Page.Add(Base85.Ascii85.Decode(String.op_Implicit(line)))

        page

    let deletePage (page:AtomPage) =  
        closeAtomFile page.PageFile
        closeAtomFile page.IndexFile
        page.Name |> getPageFileName |> deleteFile
        page.Name |> getPageIndexFileName |> deleteFile
        

    let indexToString (index:AtomIndex) = 
        String.Format(
            "{0}{1}{2}", 
            index.Key.PadRight(AtomConstants.keyLength, AtomConstants.indexPadCharacter), 
            index.Hash,
            index.DocumentLocation.ToString().PadRight(AtomConstants.indexLength, AtomConstants.indexPadCharacter)
        )                  

    let getDocumentHash (doc:byte[]) = 
        use sha512 = SHA512.Create()
        let hashBytes = sha512.ComputeHash(doc)
        BitConverter.ToString(hashBytes).Replace("-", String.Empty)

    let getIndex (page:AtomPage) (key:string) =
        match page.Index.ContainsKey(key) with
            | true -> page.Index.[key]
            | false -> createIndexFromString null

    (*
        Persist a record in the Page with its position therein referenced in the Page's Index file.
        Our key should be any 50 character string.
    *)
    let put (page:AtomPage) (key:string) (doc:byte[]) =               
        let index = getIndex page key

        let documentHash = getDocumentHash doc

        let requiresUpdate = 
            not index.Exists || not (documentHash.Equals(index.Hash))

        if requiresUpdate then
            if not index.Exists then
                page.IndexFile.Writer.BaseStream.Seek(0L, SeekOrigin.End) |> ignore

            //Move to the end of the file for appending and append our document to the page
            page.PageFile.Writer.BaseStream.Seek(0L, SeekOrigin.End) |> ignore

            //Encode our document in Base85 for uniform representation on disk
            let encodedDoc = Base85.Ascii85.Encode(ReadOnlySpan(doc))

            //Add our new document to in-memory page buffer
            page.Page.Add(doc)

            //Write our document to the Page file.
            page.PageFile.Writer.WriteLine(encodedDoc)

            let newIndex = {
                Exists = true
                Key = key
                Hash = documentHash
                DocumentLocation = page.PageFile.Writer.BaseStream.Position
            }
            
            //Add our new index to the in-memory index buffer
            if not(page.Index.ContainsKey(key)) then
                page.Index.Add(key, newIndex)
            else
                page.Index.[key] = newIndex |> ignore

            //Build our new index record with a uniform size so that we can update it easier.
            //Without a uniform size we'd have no certainty over where the adjacent index records begin and end.
            //This would mean we could potentially overwrite an existing one when updating etc. 
            let indexRecord = indexToString newIndex

            //Writing our index record line will either append or overwrite depending on our position in the file.
            page.IndexFile.Writer.WriteLine(indexRecord)

            //Commit our changes to disk
            page.IndexFile.Writer.Flush()
            page.PageFile.Writer.Flush()

    (*
        Use our key to first find the index of the document in our Page's Index file.
        Once we have the index, retrieve the document from the page using the index.
    *)
    let get (page:AtomPage) (key:string) = 
        //Find our key's index record. If not found fail.
        let index = getIndex page key
        if not index.Exists then
            failwith "Key not found."
        
        //Seek to our document using the index and retrieve our document.
        page.PageFile.Reader.BaseStream.Seek(index.DocumentLocation, SeekOrigin.Begin) |> ignore

        //Read the document line and Base85 decode it to bytes
        let doc = page.PageFile.Reader.ReadLine()
        Base85.Ascii85.Decode(String.op_Implicit(doc))
     

        

