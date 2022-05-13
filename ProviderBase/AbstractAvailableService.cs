using Grpc.Core;
using Microsoft.Extensions.Logging;

public class AvailableGrpcService : Available.AvailableBase
{
    private readonly ILogger<AvailableGrpcService> _logger;
    private readonly IAvailableStub availableStub;

    public AvailableGrpcService(ILogger<AvailableGrpcService> logger, IAvailableStub availableStub)
    {
        _logger = logger;
        this.availableStub = availableStub;
    }

    public override async Task<AvailableReply> RequestForProposal(AvailableRequest request, ServerCallContext context)
    {
        var rfp = await availableStub.GetProposals(request.Origin);

        return new AvailableReply()
        {
            RequestId = Guid.NewGuid().ToString().Replace("-", "") + " " + string.Join(",", rfp.Select(x => x.ToString()))
        };
    }
}

public interface IAvailableStub
{
    Task<List<object>> GetProposals(string origin);
}

