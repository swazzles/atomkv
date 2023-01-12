namespace AtomKV.Core

open System
open System.Collections.Generic


module AtomBytes = 

    let emptyArray<'T> = 
        let b: 'T [] = Array.zeroCreate 0
        b

    let from (buffer:Span<byte>) (size:int) =
        match buffer.Length with
            | x when x > size -> buffer.Slice(size)
            | _ -> Span<byte>(emptyArray<byte>)

    let between (buffer:Span<byte>) (start:int) (size:int) =
        match buffer.Length with
            | x when x > size -> buffer.Slice(start, size)
            | _ -> buffer



