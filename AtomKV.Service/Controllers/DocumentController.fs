namespace AtomKV.Service.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open AtomKV.Core.Types
open AtomKV.Core
open AtomKV.Service
open Microsoft.AspNetCore.Http

type QueryControllerDeps = {
    AtomTable: unit -> AtomTable
}

[<ApiController>]
[<Route("[controller]")>]
type DocumentController (table: AtomTable, atom:Atom) =
    inherit ControllerBase()

    [<HttpGet("{key}")>]
    member _.Get(key:string) =
        async {
            let req = GetRequest {
                RequestId = Guid.NewGuid()
                Table = table.Name
                Key = key
            } 

            let! promise = AtomTable.performRequest atom table req
            let! result = promise.CompletionChannel.Reader.ReadAsync().AsTask() |> Async.AwaitTask

            return
                match result with
                | GetResponse t -> t
                | _ -> failwith("Invalid response type")
        }

    [<HttpPut("{key}")>]
    member _.Put(key: string, [<FromBody>] model: PutRequestModel) =
        async {

            let requestId = Guid.NewGuid()
            let req = PutRequest {
                RequestId = requestId
                Table = table.Name
                Key = key
                Doc = model.Doc
            } 

            let! promise = AtomTable.performRequest atom table req
            let! result = promise.CompletionChannel.Reader.ReadAsync().AsTask() |> Async.AwaitTask

            return
                match result with
                | PutResponse t -> t
                | _ -> failwith("Invalid response type")
        }
