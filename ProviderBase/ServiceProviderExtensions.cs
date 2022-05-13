using System.Net;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public static class ServiceProviderExtensions
{
    public static Uri GetCurrentServerAddress(this IServiceProvider sp)
    {
        var server = sp.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>()!;
        var addresses = addressFeature.Addresses.Select(address => new Uri(address));

        return new Uri("http://" + Dns.GetHostName() + ":" + addresses.First().Port);
    }

    public static WebApplicationBuilder AddGrpcServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();

        return builder;
    }

    public static WebApplicationBuilder AddConsulDiscovery(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AgentServiceRegistration>(builder.Configuration.GetSection("AgentServiceRegistration"));
        builder.Services.AddTransient<IConsulClient>((sp) =>
        {
            return new ConsulClient(config =>
            {
                config.Address = new Uri(builder.Configuration.GetValue<string>("Discovery:Url"));
            });
        });
        return builder;
    }

    public static WebApplicationBuilder ConfigureGrpcKestrel(this WebApplicationBuilder builder, int port = 5000)
    {
        #if DEBUG
            port = new Random().Next(6000,7000);
        #endif 
        
        builder.WebHost.UseKestrel(options =>
        {
            options.ListenAnyIP(port, o => o.Protocols = HttpProtocols.Http2);
        });

        return builder;
    }

    public static WebApplicationBuilder AddStub<T>(this WebApplicationBuilder builder, T stub) where T : IAvailableStub, new()
    {
        builder.Services.AddTransient<IAvailableStub>((s) => stub);
        return builder;
    }

    public static IEndpointRouteBuilder MapGrpcEndpoint<T>(this IEndpointRouteBuilder endpoints) where T : class
    {
        endpoints.MapGrpcReflectionService();
        endpoints.MapGrpcService<T>();

        return endpoints;
    }

    public static void EnableDiscoveryRegistration(this WebApplication app, string serviceUID)
    {
        app.MapGet("/health", ([FromServices] ILogger<WebApplication> logger) =>
        {
            return "I'm running";
        });

        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            using var consul = app.Services.GetRequiredService<IConsulClient>();

            var options = app.Services.GetRequiredService<IOptions<AgentServiceRegistration>>().Value;

            var address = app.Services.GetCurrentServerAddress()!;

            options.ID = serviceUID;
            options.Address = address.Host;
            options.Port = address.Port;

            options.Check = new AgentServiceCheck()
            {
                TCP = $"{Dns.GetHostName()}:{address.Port}",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(1)
            };

            await consul.Agent.ServiceRegister(options);

        });

        // app.Lifetime.ApplicationStopping.Register(async () =>
        // {
        //     using var consul = new ConsulClient(config =>
        //     {
        //         config.Address = new Uri(app.Configuration.GetValue<string>("Discovery:Url"));
        //     });

        //     await consul.Agent.ServiceDeregister(serviceUID);

        // });
    }

    public static void MapGrpcEndpoints(this WebApplication app)
    {
        app.UseRouting();

        app.UseEndpoints(ep =>
        {
            ep.MapGrpcService<AvailableGrpcService>();
            ep.MapGrpcReflectionService();
        });
    }
}