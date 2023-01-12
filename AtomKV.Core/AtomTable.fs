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
    let openTable tableName keySpaceSize (serviceBundle:ServiceBundle) = 
        let table = {
            Name = tableName
            KeySpaceSize = keySpaceSize
            Pages = [for i in [1..keySpaceSize] do yield (i, AtomPage.openPage $"{tableName}-{i}")] |> dict
            TablePage = AtomPage.openPage $"{tableName}.table"
        }

        DocumentSerialization.serializeDocument serviceBundle.DocumentSerializer table
            |> AtomPage.put table.TablePage "TableDefinition"

        table

    let put table key value (serviceBundle:ServiceBundle) = 
        let keyShard = KeySpace.getKeyShard serviceBundle.KeySharder key table.KeySpaceSize 
        (table.Pages.[keyShard], key, value) |||> AtomPage.put
    
    let get table key (serviceBundle:ServiceBundle) = 
        let keyShard = KeySpace.getKeyShard serviceBundle.KeySharder key table.KeySpaceSize
        (table.Pages.[keyShard], key) ||> AtomPage.get

    let dropTable table = 
        for kvp in table.Pages 
            do kvp.Value |> AtomPage.deletePage

        AtomPage.deletePage table.TablePage