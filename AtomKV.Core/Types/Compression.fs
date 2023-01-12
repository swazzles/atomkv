namespace AtomKV.Core.Types

module Compression =
    type IAtomCompressor = byte[] -> byte[]
    type IAtomDecompressor = byte[] -> byte[]

    let compress (compressor:IAtomCompressor) (bytes:byte[]) =
        compressor bytes

    let decompress (decompressor:IAtomDecompressor) (bytes:byte[]) =
        decompressor bytes

