// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open AtomKV.Core.Types
open AtomKV.Core
open System.Text
open AtomKV.AtomSharp

type TestType = {
    TestField: int
}

let measure (f:(unit -> Async<unit>)) =
    async {
        let startt = DateTime.Now
        do! f()
        let endt = DateTime.Now
        return endt - startt
    }    

let runTest = 
    async {
        let atom = AtomDefaultInitialization.initialize()
    
        let! page = AtomPage.getPage "1"
        do! AtomPage.deletePage page

        let! page = AtomPage.getPage "1"

        let docObject = {
            TestField = 1
        }

        let doc = JsonSerialization.serialize docObject

        let count = 1000
        
        let! span = measure (fun () ->
            async {
                for i in [1..count] do
                    let key = $"mykey-{i}"
                    do! AtomPage.put atom page key doc |> Async.Ignore
            }
        )

        System.Console.WriteLine($"Finished {count} puts in {span}")

        let! span = measure (fun () ->
            async {
                for i in [1..count] do
                    let key = $"mykey-{i}"
                    let! getResponse = AtomPage.get atom page key
                    let newDoc = getResponse |> AtomDocument.unwrap 
                    newDoc |> JsonSerialization.deserialize<TestType> |> ignore   
            }
        )

        System.Console.WriteLine($"Finished {count} gets in {span}")
        

        
    }

[<EntryPoint>]
let main argv =
    runTest |> Async.RunSynchronously
    0 // return an integer exit code    