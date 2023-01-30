namespace AtomKV.Core

open AtomKV.Core.Types
open Microsoft.Extensions.Hosting
open System.Threading
open System.Threading.Tasks
open System.Threading.Channels

module AtomBackgroundService = 
    type AtomWorker(
        atom:Atom, 
        table:AtomTable
        ) = 

        [<DefaultValue>]
        val mutable task : Task

        let tableCancellationToken = new CancellationTokenSource()
        
        interface IHostedService with
            member this.StartAsync(cancellationToken: CancellationToken): Task =   
                this.task <- AtomTable.run atom table tableCancellationToken.Token |> Async.StartAsTask
                Task.CompletedTask

            member this.StopAsync(cancellationToken: CancellationToken): Task = 
                tableCancellationToken.Cancel()
                Task.CompletedTask

        let getTable() = 
            table

