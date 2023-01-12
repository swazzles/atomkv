namespace AtomKV.Core.Types

open System.Collections.Generic

type ServiceBundle = {
    DocumentSerializer: DocumentSerialization.IDocumentSerializer
    DocumentDeserializer: DocumentSerialization.IDocumentDeserializer

    KeyHasher: KeySpace.IKeyHasher
    KeySharder: KeySpace.IKeySharder
    KeyValidator: KeySpace.IKeyValidator

    Compressor: Compression.IAtomCompressor
    Decompressor: Compression.IAtomDecompressor
}

