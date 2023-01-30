namespace AtomKV.Core

open AtomKV.Core.Types
open AtomKV.Core

module AtomDefaultInitialization = 
    let initialize() = 
        {
            KeyHasher = AtomKeySpaceV1.getKeyHash
            KeySharder = AtomKeySpaceV1.getKeyShard
            KeyValidator = AtomKeySpaceV1.validateKey
        }