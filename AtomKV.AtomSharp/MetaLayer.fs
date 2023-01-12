namespace AtomKV.AtomSharp

open AtomKV.Core.Types
open AtomKV.Core

module MetaLayer = 
    type MetaLayerState() =
        member val ActiveTable:AtomTable option = None with get, set
        member val Services:ServiceBundle = {
            DocumentSerializer = JsonDocumentSerialization.objectToJsonBytes
            DocumentDeserializer = JsonDocumentSerialization.objectFromJsonBytes

            KeyHasher = AtomKeySpaceV1.getKeyHash
            KeySharder = AtomKeySpaceV1.getKeyShard
            KeyValidator = AtomKeySpaceV1.validateKey

            Compressor = GZipCompression.compress
            Decompressor = GZipCompression.decompress
        }

    let state = new MetaLayerState()

    let table name =
        state.ActiveTable <- Some(AtomTable.openTable name 16 state.Services)

    let put key value = 
        match state.ActiveTable with
        | Some(table) -> AtomTable.put table key value state.Services
        | _ -> ()

    let get key = 
        match state.ActiveTable with
        | Some(table) -> AtomTable.get table key state.Services |> DocumentSerialization.serializeDocument state.Services.DocumentSerializer


