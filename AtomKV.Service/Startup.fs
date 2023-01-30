namespace AtomKV.Service

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open AtomKV.Core.Types
open AtomKV.Core

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member _.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddControllers() |> ignore

        services.AddSwaggerGen() |> ignore

        services.AddSingleton<Atom>(AtomDefaultInitialization.initialize())  |> ignore

        services.AddSingleton<AtomTable>(
            fun sp -> 
                let atom = sp.GetService<Atom>()
                AtomTable.openTable "MyTable" 32
            )  |> ignore

        services.AddHostedService(
            fun sp ->
                let atom = sp.GetService<Atom>()
                let table = sp.GetService<AtomTable>()
                new AtomBackgroundService.AtomWorker(atom, table)
        )  |> ignore

        services.AddSingleton<IAtomInitializer>(AtomDefaultInitialization.initialize) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage()
                .UseSwagger()
                .UseSwaggerUI(fun opts ->
                    opts.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    opts.RoutePrefix <- String.Empty
                ) 
                |> ignore
        app.UseHttpsRedirection()
           .UseRouting()
           .UseAuthorization()
           .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
            ) |> ignore            
