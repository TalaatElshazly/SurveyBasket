namespace Bookify.Services;

public interface IJwtService
{
    (string token,int expiresIn) GenerateJwtToken(ApplicationUser user, IEnumerable<string> roles);
    string? GetUserIdFromToken(string token);
}
