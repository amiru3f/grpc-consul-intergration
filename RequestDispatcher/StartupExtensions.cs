using Consul;
using Grpc.Core;
using Grpc.Net.Client.Configuration;

namespace RequestDispatcher;
public static class StartupExtensions
{
    public static async Task AddGrpcClients(this IServiceCollection services, IConfiguration configuration)
    {
        using var consul = new ConsulClient(consulConfig =>
        {
            consulConfig.Address = new Uri(configuration.GetValue<string>("Discovery:Url"));
        });

        var registeredProvidersResponse = await consul.Agent.Services();
        var registeredGrpcProviders = registeredProvidersResponse.Response.Values
            .Where(x => x.Tags.Contains("provider") && x.Tags.Contains("grpc"));

        foreach (var provider in registeredGrpcProviders)
        {
            if (provider.Tags.Contains("http"))
            {
                provider.Address = "http://" + provider.Address;
            }
            else
            {
                provider.Address = "https://" + provider.Address;
            }

            services.AddGrpcClient<Available.AvailableClient>(provider.Service, o =>
            {
                o.Address = new Uri(provider.Address + ":" + provider.Port);
                o.ChannelOptionsActions.Add(new Action<Grpc.Net.Client.GrpcChannelOptions>((action) =>
                {
                    action.ServiceConfig = new ServiceConfig
                    {
                        MethodConfigs =
                        {
                                new Grpc.Net.Client.Configuration.MethodConfig()
                                {
                                    RetryPolicy = new Grpc.Net.Client.Configuration.RetryPolicy()
                                    {
                                        MaxAttempts = 5,
                                        InitialBackoff = TimeSpan.FromSeconds(1),
                                        MaxBackoff = TimeSpan.FromSeconds(5),
                                        BackoffMultiplier = 1.5,
                                        RetryableStatusCodes = { StatusCode.Unavailable }
                                    }
                                }
                        }
                    };

                    action.HttpHandler = new SocketsHttpHandler
                    {
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                        EnableMultipleHttp2Connections = true
                    };
                }));
            });
        }
    }
}

