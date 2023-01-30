namespace AtomKV.Core

module AtomIndex = 
    type AtomIndexData = {
        Key:string
        Hash:string
        DocumentStart:int64
        DocumentLength: int64
    }

    type AtomIndex = {
        IndexLocation: int64
        Data: AtomIndexData
    }

    let indexToString (index:AtomIndexData) = 
        $"{index.Key}|{index.Hash}|{index.DocumentStart}|{index.DocumentLength}"

    let indexFromString (index:string) =        
        let tokens = index.Split "|"
        {
            Key = tokens[0]
            Hash = tokens[1]
            DocumentStart = int64(tokens[2])
            DocumentLength = int64(tokens[3])
        }

    let requiresUpdate index hash = 
        match index with
        | Some(x) -> not (hash.Equals(x.Data.Hash))
        | None -> true

