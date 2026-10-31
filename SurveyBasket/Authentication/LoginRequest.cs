namespace Bookify.Authentication;

public record LoginRequest(
    string Email,
    string Password
);