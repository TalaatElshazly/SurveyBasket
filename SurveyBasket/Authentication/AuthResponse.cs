namespace Bookify.Authentication;

public record AuthResponse(
    string Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string Token,
    int ExpiresInMinutes,
    string RefreshToken,
    DateTime RefreshTokenExpiration
    );