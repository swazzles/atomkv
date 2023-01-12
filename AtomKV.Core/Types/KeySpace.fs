namespace AtomKV.Core.Types

module KeySpace = 
    type IKeyHasher = string -> string
    type IKeySharder = string -> int -> int
    type IKeyValidator = string -> bool

    let getKeyHash (hasher:IKeyHasher) key =
        hasher key

    let getKeyShard (sharder:IKeySharder) key =
        sharder key

    let isKeyValid (validator:IKeyValidator) key = 
        validator key