// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open AtomKV.Core.Types
open AtomKV.Core
open System.Text
open AtomKV.AtomSharp

[<EntryPoint>]
let main argv =
    let mutable running = true
    let session = Parser.getSession
    while running do
        System.Console.Write("AtomKV: ")
        let input = System.Console.ReadLine()
        let result = Parser.parse session input
        System.Console.WriteLine(result)

    0 // return an integer exit code    