using Microsoft.AspNetCore.Identity;

namespace Bookify.Services;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse?> LoginAsync(LoginRequest? request, CancellationToken cancellationToken);
    Task<AuthResponse?> GetRefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task<bool> RevokeTokenAsync(RevokeRefresheTokenRequest request, CancellationToken cancellationToken);
}