namespace BoulderingRecordAPI.Models.Auth;

public record LoginResponse(string Token, DateTimeOffset ExpiresAt);
