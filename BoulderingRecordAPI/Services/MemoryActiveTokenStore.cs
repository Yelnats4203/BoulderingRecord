using Microsoft.Extensions.Caching.Memory;

namespace BoulderingRecordAPI.Services;

public class MemoryActiveTokenStore(IMemoryCache cache) : IActiveTokenStore
{
    private static string BuildKey(string acc) => $"active-token:{acc}";

    public void SetActiveToken(string acc, string token, DateTimeOffset expiresAt)
        => cache.Set(BuildKey(acc), new ActiveTokenEntry(token, expiresAt), expiresAt);

    public bool TryGetActiveToken(string acc, out string? token)
    {
        if (cache.TryGetValue(BuildKey(acc), out ActiveTokenEntry? entry) && entry is not null)
        {
            token = entry.Token;
            return true;
        }

        token = null;
        return false;
    }

    public void RemoveActiveToken(string acc) => cache.Remove(BuildKey(acc));
}
