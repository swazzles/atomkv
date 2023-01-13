namespace AtomKV.Core

open System.IO
open System.Text

open AtomKV.Core.Types

module AtomFile = 
    type AtomFile = {
        Reader: StreamReader
        Writer: StreamWriter
    }

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

    let rec private searchInStream (reader:StreamReader) (predicator:string -> bool) = 
        if reader.EndOfStream then
            (-1L, None)
        else       
            let location = reader.BaseStream.Position
            let line = reader.ReadLine()
            if predicator line then
                (location, Some(line))
            else
                searchInStream reader predicator

    let search (file:AtomFile) (predicator:string->bool) =
        file.Reader.BaseStream.Seek(0, SeekOrigin.Begin) |> ignore
        searchInStream file.Reader predicator        
                
    let appendBytes (atom:Atom) (file:AtomFile) (data:byte[]) = 
        let writeStart = file.Writer.BaseStream.Seek(0L, SeekOrigin.End)
        let compressedData = Compression.compress atom.Compressor data
        for b in compressedData do 
            file.Writer.Write(char(b))
        writeStart

    let appendLine file (line:string) =
        file.Writer.BaseStream.Seek(0L, SeekOrigin.End) |> ignore
        file.Writer.WriteLine(line)

    let writeLineAt file location (line:string) =
        file.Writer.BaseStream.Seek(location, SeekOrigin.Begin) |> ignore
        file.Writer.WriteLine(line)

    let commit file =
        file.Writer.Flush()

    let readBytes (atom:Atom) file start length = 
        file.Reader.BaseStream.Seek(start, SeekOrigin.Begin) |> ignore
        [for i in 0L..length do yield byte(char(file.Reader.Read()))] 
            |> List.toArray
            |> Compression.decompress atom.Decompressor
