namespace AtomKV.AtomSharp

open AtomKV.Core.Types
open AtomKV.Core

module MetaLayer = 
    type MetaLayerState() =
        member val ActiveTable:AtomTable option = None with get, set
        member val Atom:Atom = AtomDefaultInitialization.initialize()

    let state = new MetaLayerState()

    let table name =
        state.ActiveTable <- Some(AtomTable.openTable state.Atom name 16 |> Async.RunSynchronously)

    let put key value = 
        match state.ActiveTable with
        | Some(table) -> AtomTable.put state.Atom table key value |> Async.RunSynchronously
        | _ -> failwith "No active table."

    let get key = 
        match state.ActiveTable with
        | Some(table) -> Some(AtomTable.get state.Atom table key |> Async.RunSynchronously)
        | _ -> failwith "No active table."


