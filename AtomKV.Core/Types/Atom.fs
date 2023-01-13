namespace AtomKV.Core.Types

open System.Collections.Generic

type Atom = {
    KeyHasher: KeySpace.IKeyHasher
    KeySharder: KeySpace.IKeySharder
    KeyValidator: KeySpace.IKeyValidator

    Compressor: Compression.IAtomCompressor
    Decompressor: Compression.IAtomDecompressor
}

type IAtomInitializer = unit -> Atom

module Atom =
    let initialize (initializer:IAtomInitializer) =
        initializer()
    

