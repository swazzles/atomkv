namespace AtomKV.Core

open System.Collections.Generic
open System.Text.Json.Serialization
open AtomKV.Core.Types

type AtomTable = {
    Name: string
    KeySpaceSize: int

    [<JsonIgnore>]
    TablePage: AtomPage

    [<JsonIgnore>]
    Pages: IDictionary<int, AtomPage>
}

module AtomTable = 
    let openTable (atom:Atom) tableName keySpaceSize = 
        let table = {
            Name = tableName
            KeySpaceSize = keySpaceSize
            Pages = [for i in [1..keySpaceSize] do yield (i, AtomPage.openPage $"{tableName}-{i}")] |> dict
            TablePage = AtomPage.openPage $"{tableName}.table"
        }

        let res = JsonSerialization.serialize table |> AtomPage.put atom table.TablePage "TableDefinition"
        match res.Status with
        | PutResponseStatus.Ok | PutResponseStatus.NoUpdate -> table
        | PutResponseStatus.Fail -> failwith $"Error when opening table {tableName}. See error logs."   

    let put (atom:Atom) table key doc = 
        let keyShard = KeySpace.getKeyShard atom.KeySharder key table.KeySpaceSize 
        AtomPage.put atom table.Pages.[keyShard] key doc
    
    let get (atom:Atom) table key = 
        let keyShard = KeySpace.getKeyShard atom.KeySharder key table.KeySpaceSize
        AtomPage.get atom table.Pages.[keyShard] key 

    let dropTable (atom:Atom) table = 
        for kvp in table.Pages 
            do kvp.Value |> AtomPage.deletePage

        AtomPage.deletePage table.TablePage