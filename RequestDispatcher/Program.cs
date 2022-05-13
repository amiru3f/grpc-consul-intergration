using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Consul;
using RequestDispatcher;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IConsulClient>((sp) =>
{
    return new ConsulClient(config =>
    {
        config.Address = new Uri(builder.Configuration.GetValue<string>("Discovery:Url"));
    });
});

await builder.Services.AddGrpcClients(builder.Configuration);

var app = builder.Build();

app.MapGet("/test", async ([FromServices] GrpcClientFactory grpcClientFactory, [FromServices] IConsulClient consulClient, CancellationToken cancellationToken) =>
{

    // var filter =
    //     !serviceFilter.Service.Tags.IsEmpty() &
    //     serviceFilter.Service.Tags.Contains("grpc") &
    //     serviceFilter.Service.Tags.Contains("provider");

    var servicesResponse = await consulClient.Agent.Services(cancellationToken);

    StringBuilder stringBuilder = new StringBuilder();
    foreach (var provider in servicesResponse.Response.Values.Where(x => x.Tags.Contains("grpc") && x.Tags.Contains("provider")).Select(x => x.Service))
    {
        var providerClient = grpcClientFactory.CreateClient<Available.AvailableClient>(provider);

        var result = await providerClient.RequestForProposalAsync(new AvailableRequest()
        {
            Origin = "THR"
        });

        stringBuilder.AppendLine(result.RequestId);
    }

    return stringBuilder.ToString();
});


await app.RunAsync();