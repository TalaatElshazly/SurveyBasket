namespace Bookify.Authentication;

public record RefreshTokenRequest(string Token, string RefreshToken);
