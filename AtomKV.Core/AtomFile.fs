namespace AtomKV.Core

open System.IO
open AtomKV.Core.Types

module AtomFile = 
    type AtomFile = {
        Stream: FileStream
    }
    
    let openFile (fileName:string) = 
        {
            Stream = File.Open(fileName, FileMode.OpenOrCreate)
        }

    let closeFile file = 
        file.Stream.Dispose()

    let deleteFile (fileName:string) =
        match File.Exists fileName with
            |true -> File.Delete fileName
            |false -> ()

    (*
        Iterates recursively line-by-line through a stream and performs the predicator on said line.
        Returns the first occurence of a line where the result of predicator = true
    *)
    let rec private searchInStream (file:StreamReader) (predicator:string -> bool) = 
        if file.EndOfStream then
            (-1L, None)
        else       
            let location = file.BaseStream.Position
            let line = file.ReadLine()
            if predicator line then
                (location, Some(line))
            else
                searchInStream file predicator

    (*
        Searches through a file and finds a line where predicator = true
    *)
    let search file (predicator:string->bool) =
        file.Stream.Seek(0L, SeekOrigin.Begin) |> ignore

        let fileReader = new StreamReader(file.Stream)
        let (location, line) = searchInStream fileReader predicator

        (location, line)
                
    let appendBytes (atom:Atom) file (data:byte[]) = 
        let writeStart = file.Stream.Seek(0L, SeekOrigin.End)
        file.Stream.Write(data)
        file.Stream.Flush()
        (writeStart, data.Length)

    let readBytes (atom:Atom) file start (length:int64) = 
        file.Stream.Seek(start, SeekOrigin.Begin) |> ignore
        [for i in [0L..length] do yield byte(file.Stream.ReadByte())] |> Seq.toArray

    let appendLine file (line:string) =
        let location = file.Stream.Seek(0L, SeekOrigin.End)
        let fileWriter = new StreamWriter(file.Stream)
        fileWriter.WriteLine(line)
        fileWriter.Flush()
        location

    let writeLineAt file location (line:string) =
        file.Stream.Seek(location, SeekOrigin.Begin) |> ignore            
        let fileWriter = new StreamWriter(file.Stream)
        fileWriter.WriteLine(line)
        fileWriter.Flush()


