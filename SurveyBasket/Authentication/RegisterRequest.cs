namespace Bookify.Authentication;

public record RegisterRequest(
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string Password
);