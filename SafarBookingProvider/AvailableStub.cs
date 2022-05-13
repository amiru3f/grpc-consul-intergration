
public class AvailableRequestHandler : IAvailableStub
{
    public Task<List<object>> GetProposals(string origin)
    {
        return Task.FromResult(new List<object>() { "Safar booking available request executed" });
    }
}

