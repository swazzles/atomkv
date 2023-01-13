namespace AtomKV.Core.Types

type PutResponseStatus =
    | Ok
    | NoUpdate
    | Fail

type PutResponse = {
    Status: PutResponseStatus
}


type GetResponseStatus = 
    | Ok
    | KeyNotExist
    | Fail

type GetResponse = {
    Status: GetResponseStatus
    Document: byte[] option
}

