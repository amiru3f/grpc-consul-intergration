var builder = WebApplication.CreateBuilder(args);

builder
        .ConfigureGrpcKestrel(port: 5001)
        //can be changed (refactored)
        .AddStub(new AvailableStub())
        .AddGrpcServices()
        .AddConsulDiscovery();


var app = builder.Build();

app.UseRouting();
app.UseEndpoints(ep =>
{
    ep.MapGrpcService<AvailableGrpcService>();
    ep.MapGrpcReflectionService();
});

app.EnableDiscoveryRegistration("amadeus-provider");
await app.RunAsync();