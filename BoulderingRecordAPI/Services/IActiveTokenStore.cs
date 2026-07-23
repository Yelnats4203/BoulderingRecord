namespace BoulderingRecordAPI.Services;

public interface IActiveTokenStore
{
    void SetActiveToken(string acc, string token, DateTimeOffset expiresAt);

    bool TryGetActiveToken(string acc, out string? token);

    void RemoveActiveToken(string acc);
}

public record ActiveTokenEntry(string Token, DateTimeOffset ExpiresAt);
