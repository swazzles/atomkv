namespace AtomKV.Core

open System.Collections.Generic
open System.Text.Json.Serialization
open AtomKV.Core.Types
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open System.Collections.Concurrent
open System

type AtomTable = {
    Name: string
    KeySpaceSize: int
    Pages: IDictionary<string, AtomPage>
    RequestChannel: Channel<AtomRequest>
}

module AtomTable =
    let private getPageFileName tableName keySpaceSize i = 
        $"{tableName}-{keySpaceSize}-{i}"

    (*
        Create/open an AtomPage for each partition in the keyspace.
    *)
    let private openPages tableName keySpaceSize =
        let shards = [for i in [1..keySpaceSize] -> getPageFileName tableName keySpaceSize i]
        let pages = shards |> Seq.map AtomPage.getPage
        [for page in pages do yield (page.Name, page) ] |> dict

    let private closeTable table = 
        async {
            do! table.Pages
                |> Seq.map (fun t -> AtomPage.closePage t.Value)
                |> Async.Parallel
                |> Async.Ignore
        }

    let openTable tableName keySpaceSize = 
        let pages = openPages tableName keySpaceSize
        {
            Name = tableName
            KeySpaceSize = keySpaceSize
            Pages = pages
            RequestChannel = Channel.CreateBounded<AtomRequest>(keySpaceSize*AtomConstants.pageChannelBounding)
        }

    (*
        Main table thread.
        Starts up a thread for each page performing processRequestsAsync for each.
        Closes the table and each thread upon cancellation.
    *)
    let run (atom:Atom) (table:AtomTable) (cancellationToken:CancellationToken)  = 
        async {
            [for page in table.Pages do yield AtomPage.processRequestsAsync atom page.Value cancellationToken]
                |> Async.Parallel
                |> Async.RunSynchronously
                |> ignore

            do! closeTable table
        }   

    let performRequest (atom:Atom) (table:AtomTable) (req:AtomRequest) = 
        async {
            let key =
                match req with
                | PutRequest(t) -> t.Key
                | GetRequest(t) -> t.Key

            let keyShard = KeySpace.getKeyShard atom.KeySharder key table.KeySpaceSize
            let pageFilePath = getPageFileName table.Name table.KeySpaceSize keyShard
            let page = table.Pages.[pageFilePath]

            let promise = {
                Request = req
                CompletionChannel = Channel.CreateBounded<AtomResponse>(1)
            }

            do! page.RequestChannel.Writer.WriteAsync(promise).AsTask() |> Async.AwaitTask

            return promise
        }