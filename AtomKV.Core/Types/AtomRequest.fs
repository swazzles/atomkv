namespace AtomKV.Core.Types

open System
open System.Threading.Channels

type PutRequest = {
    RequestId: Guid
    Table: string
    Key: string
    Doc: byte[]
}

type PutResponseStatus =
    | Ok = 0
    | NoUpdate = 1
    | Fail = 2
    | TimedOut = 3

type PutResponse = {
    RequestId: Guid 
    Status: PutResponseStatus
}


type GetRequest = {
    RequestId: Guid
    Table: string
    Key: string
}

type GetResponseStatus = 
    | Ok = 0
    | KeyNotExist = 1
    | Fail = 2
    | TimedOut = 3

type GetResponse = {
    RequestId: Guid
    Status: GetResponseStatus
    Document: byte[] option
}

type AtomRequest = 
    | PutRequest of PutRequest
    | GetRequest of GetRequest

type AtomResponse = 
    | PutResponse of PutResponse
    | GetResponse of GetResponse

type AtomRequestPromise = 
    {
        Request: AtomRequest
        CompletionChannel: Channel<AtomResponse>
    }