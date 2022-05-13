public class AvailableStub : IAvailableStub
{
    public Task<List<object>> GetProposals(string origin)
    {
        return Task.FromResult(new List<object>() { "I'm from Airtour" });
    }
}