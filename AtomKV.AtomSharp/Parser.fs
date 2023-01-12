namespace AtomKV.AtomSharp

open System
open System.IO
open FSharp.Compiler.Interactive
open AtomKV.Core.Types
open AtomKV.Core

module Parser =

    type Session = {
        FsiSession: Shell.FsiEvaluationSession
    }

    let getSession = 
        let textReader = new StreamReader(new MemoryStream())
        let textWriter = new StreamWriter(new MemoryStream())
        let errorWriter = new StreamWriter(new MemoryStream())
        let args = [|""|]
        let session = Shell.FsiEvaluationSession.Create(Shell.FsiEvaluationSession.GetDefaultConfiguration(), args, textReader, textWriter, errorWriter)
        session.AddBoundValue("table", MetaLayer.table)
        session.AddBoundValue("get", MetaLayer.get)
        session.AddBoundValue("put", MetaLayer.put)
        
        {
            FsiSession = session
        }

    let matchFsiValue (opt:Shell.FsiValue option) =
        match opt with
            | Some(x) -> x.ReflectionValue.ToString()
            | _ -> ""

    let matchFsiValueChoice (choice:Choice<Shell.FsiValue option,exn>) =
        match choice with
        | Choice1Of2 t -> matchFsiValue t
        | Choice2Of2 t -> t.Message

    let parse session script =                 
        let (fsiValueChoice, diagnostic) = session.FsiSession.EvalExpressionNonThrowing(script)
        match fsiValueChoice with
        | Choice1Of2 t -> matchFsiValue t
        | Choice2Of2 t -> diagnostic |> Array.map (fun x -> x.ToString()) |> Array.reduce (fun a b -> $"{a}\n{b}")
        
        
