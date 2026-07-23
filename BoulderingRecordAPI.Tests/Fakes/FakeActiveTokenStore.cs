using BoulderingRecordAPI.Services;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeActiveTokenStore : IActiveTokenStore
{
    private readonly Dictionary<string, string> _tokensByAcc = [];

    public void SetActiveToken(string acc, string token, DateTimeOffset expiresAt) => _tokensByAcc[acc] = token;

    public bool TryGetActiveToken(string acc, out string? token) => _tokensByAcc.TryGetValue(acc, out token);

    public void RemoveActiveToken(string acc) => _tokensByAcc.Remove(acc);
}
