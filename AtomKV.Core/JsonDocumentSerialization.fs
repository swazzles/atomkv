namespace AtomKV.Core

open System.Text
open System.Text.Json

module JsonDocumentSerialization = 

    let objectToJsonBytes obj =
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj))

    let objectFromJsonBytes<'T> (bytes:byte[]) = 
        JsonSerializer.Deserialize<'T>(Encoding.UTF8.GetString(bytes))
        