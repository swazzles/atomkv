namespace AtomKV.Core.Types

open System.Text
open System.Text.Json

module DocumentSerialization = 

    type IDocumentSerializer = obj -> byte[]
    type IDocumentDeserializer = byte[] -> obj

    type Dependency<'T>(def:'T) = member val D = def with get, set

    let serializeDocument (serializer:IDocumentSerializer) obj = 
        serializer obj

    let deserializeDocument (deserializer:IDocumentDeserializer) bytes = 
        deserializer bytes
        