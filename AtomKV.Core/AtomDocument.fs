namespace AtomKV.Core

open AtomKV.Core.Types

module AtomDocument = 
    let unwrap (getResponse:GetResponse) =
        match getResponse.Document with
        | Some(x) -> x
        | None -> failwith "Document is None and cannot be unwrapped."
