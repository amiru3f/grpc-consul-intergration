
public class ArminRequestHandler : IAvailableStub
{
    public Task<List<object>> GetProposals(string origin)
    {
        return Task.FromResult(new List<object>() { "Armin's available request executed" });
    }
}

