var builder = WebApplication.CreateBuilder(args);

builder
        .ConfigureGrpcKestrel(port: 5005)
        //can be changed (refactored)
        .AddStub(new AvailableStub())
        .AddGrpcServices()
        .AddConsulDiscovery();


var app = builder.Build();

app.MapGrpcEndpoints();

app.EnableDiscoveryRegistration("airtour-provider");
await app.RunAsync();