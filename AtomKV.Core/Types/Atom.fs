namespace AtomKV.Core.Types

type Atom = {
    KeyHasher: KeySpace.IKeyHasher
    KeySharder: KeySpace.IKeySharder
    KeyValidator: KeySpace.IKeyValidator

}

type IAtomInitializer = unit -> Atom

module Atom =
    let initialize (initializer:IAtomInitializer) =
        initializer()
    

