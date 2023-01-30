namespace AtomKV.Core

open System.Text
open System.Text.Json

module JsonSerialization = 

    let serialize obj =
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj))

    let deserialize<'T> (bytes:byte[]) =
        JsonSerializer.Deserialize<'T>(Encoding.UTF8.GetString(bytes))
        