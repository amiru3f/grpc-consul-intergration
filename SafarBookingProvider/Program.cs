var builder = WebApplication.CreateBuilder(args);

builder
        .ConfigureGrpcKestrel(port: 5003)
        //can be changed (refactored)
        .AddStub(new AvailableRequestHandler())
        .AddGrpcServices()
        .AddConsulDiscovery();


var app = builder.Build();

app.MapGrpcEndpoints();

app.EnableDiscoveryRegistration("safar-booking-provider");
await app.RunAsync();