namespace AtomKV.Core

module AtomIndex = 
    type AtomIndex = {
        IndexLocation: int64
        Key:string
        Hash:string
        DocumentStart:int64
        DocumentLength: int64
    }

    let indexToString (index:AtomIndex) = 
        $"{index.Key}|{index.Hash}|{index.DocumentStart}|{index.DocumentLength}"

    let indexFromString location (index:string) = 
        let tokens = index.Split "|"
        {
            IndexLocation = location
            Key = tokens[0]
            Hash = tokens[1]
            DocumentStart = int64(tokens[2])
            DocumentLength = int64(tokens[3])
        }

    let requiresUpdate index hash = 
        match index with
        | Some(x) -> not (hash.Equals(x.Hash))
        | None -> true

