namespace AtomKV.Core.Types

module Compression =
    type IAtomCompressor = byte[] -> Async<byte[]>
    type IAtomDecompressor = byte[] -> Async<byte[]>

    let compress (compressor:IAtomCompressor) (bytes:byte[]) =
        async {
            return! compressor bytes
        }

    let decompress (decompressor:IAtomDecompressor) (bytes:byte[]) =
        async {
            return! decompressor bytes
        }

