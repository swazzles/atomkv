namespace AtomKV.Core

open System.IO
open System.IO.Compression

module GZipCompression =
    let compress (content:byte[]) =
        use outStream = new MemoryStream()
        use inStream = new MemoryStream(content)
        use gzipStream = new GZipStream(outStream, CompressionMode.Compress)
        inStream.CopyTo(gzipStream)
        outStream.ToArray()


    let decompress (content:byte[]) =        
        use outStream = new MemoryStream()
        use inStream = new MemoryStream(content)
        use gzipStream = new GZipStream(inStream, CompressionMode.Decompress) 
        gzipStream.CopyTo(outStream)
        outStream.ToArray()

