namespace AtomKV.Core

open System.IO
open System.IO.Compression

module GZipCompression =
    let compress (content:byte[]) =
        use bytes = new MemoryStream(content)
        (
            use compStream = new GZipStream(bytes, CompressionMode.Compress)
            (
                use buffStream = new BufferedStream(compStream, 64*1024)
                (
                    buffStream.Write(content, 0, content.Length)
                )
            )
            bytes.ToArray()
        )             



    let decompress (content:byte[]) =        
        use bytes = new MemoryStream(content)
        (
            use newBytes = new MemoryStream()
            use compStream = new GZipStream(bytes, CompressionMode.Decompress)
            (                
                use buffStream = new BufferedStream(compStream, 64*1024)
                (
                    buffStream.CopyTo(newBytes)
                )
            )
            newBytes.ToArray()
        )             

