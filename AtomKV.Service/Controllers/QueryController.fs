namespace AtomKV.Service.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open AtomKV.Service

[<ApiController>]
[<Route("[controller]")>]
type QueryController (logger : ILogger<QueryController>) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get(key:string) =
        
