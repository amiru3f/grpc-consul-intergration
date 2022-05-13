using Grpc.Net.ClientFactory;

public class Customized : GrpcClientFactory
{
    public override TClient CreateClient<TClient>(string name)
    {
        throw new NotImplementedException();
    }
}