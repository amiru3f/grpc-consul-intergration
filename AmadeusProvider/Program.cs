var builder = WebApplication.CreateBuilder(args);

builder
        .ConfigureGrpcKestrel()
        //can be changed (refactored)
        .AddStub(new AvailableStub())
        .AddGrpcServices()
        .AddConsulDiscovery();


var app = builder.Build();
app.MapGrpcEndpoints();
app.EnableDiscoveryRegistration("amadeus-provider");

await app.RunAsync();