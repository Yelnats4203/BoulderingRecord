namespace BoulderingRecordAPI.Models.Auth;

public record RefreshTokenResponse(string Token, DateTimeOffset ExpiresAt);
