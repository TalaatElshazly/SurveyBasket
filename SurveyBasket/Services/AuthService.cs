using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Cryptography;
namespace Bookify.Services;


public class AuthService(UserManager<ApplicationUser> userManager,
                   RoleManager<IdentityRole> roleManager,
                   SignInManager<ApplicationUser> signInManager,
                   IJwtService jwtService,
                   IOptions<JwtOptions> jwtOptions,
                   ISecurityAuditService auditService) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtService _jwtService = jwtService;
    private readonly JwtOptions _jwtOptions=jwtOptions.Value;
    private readonly ISecurityAuditService _auditService = auditService;
    public async Task<IdentityResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken=default)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            Email = request.Email,
        };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
            await _userManager.AddToRoleAsync(user, "User");
            await _auditService.LogAuthEventAsync(SecurityEvents.Register, request.Email,
       isSuccess: true, userId: user.Id);
        }
        else
        {
            var errorCodes = string.Join(", ", result.Errors.Select(e => e.Code));
            await _auditService.LogAuthEventAsync(SecurityEvents.Register, request.Email,
                isSuccess: false, failureReason: AuthFailureReason.RegistrationFailed, details: errorCodes);
        }

        return result;
    }
    public async Task<AuthResponse?> LoginAsync(LoginRequest? request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return null;

        var user = await 
            _userManager.Users.Include(x => x.RefreshTokens)
                        .SingleOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
        if (user == null)
        {
            await _auditService.LogAuthEventAsync
                (SecurityEvents.Login,
                request.Email,
                false,
                AuthFailureReason.UserNotFound);
            return null;
        }
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            var reason = signInResult.IsLockedOut
                ? AuthFailureReason.LockedOut
                : AuthFailureReason.InvalidPassword;
            await _auditService.LogAuthEventAsync(
                SecurityEvents.Login,
                request.Email,
                isSuccess: false,
                failureReason: reason,
                userId: user.Id);
            return null;
        }
        var userRoles = await _userManager.GetRolesAsync(user);

        var (token, expiresIn) = _jwtService.GenerateJwtToken(user, userRoles);

        var refreshToken = GenerateRefreshToken();
        var refreshTokenEpiration = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);
        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresOn = refreshTokenEpiration
        });
        var cutoff = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenCutoffDays);
        user.RefreshTokens.RemoveAll(t => !t.IsActive && (t.RevokedOn ?? t.ExpiresOn) < cutoff);
        await _userManager.UpdateAsync(user);
        await _auditService.LogAuthEventAsync
            (SecurityEvents.Login,
            request.Email,
            true,
            null,
            user.Id);
        return new AuthResponse
        (
             user.Id,
            user.Email ?? string.Empty,
             user.UserName ?? string.Empty,
             user.FirstName,
             user.LastName,
            token,
            expiresIn,
            refreshToken,
            refreshTokenEpiration

        );
    }
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<AuthResponse?> GetRefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token)
            || string.IsNullOrWhiteSpace(request.RefreshToken))
            return null;

        var userId = _jwtService.GetUserIdFromToken(request.Token);
        
        if (userId is null)
        {
            await _auditService.LogAuthEventAsync
            (SecurityEvents.TokenRefresh,
            null,
            false,
            AuthFailureReason.InvalidAccessToken,
            null);
            return null;
        }
        
        var user = await _userManager.Users
       .Include(x => x.RefreshTokens)
       .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        
        if (user is null)
        {
            await _auditService.LogAuthEventAsync
                (SecurityEvents.TokenRefresh,
                null,
                false,
                AuthFailureReason.UserNotFound,
                userId
                );
            return null;
        }
        
        var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == request.RefreshToken);
        
        if (refreshToken is null)
        {
            await _auditService.LogAuthEventAsync
                (SecurityEvents.TokenRefresh,
                user.Email,
                false,
                AuthFailureReason.RefreshTokenNotFound,
                user.Id);
            return null;
        }
        
        if (!refreshToken.IsActive)
        {
            if (refreshToken.RevokedOn is not null)
            {
                foreach (var t in user.RefreshTokens.Where(t => t.IsActive))
                    t.RevokedOn = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _auditService.LogAuthEventAsync
                    (SecurityEvents.TokenRefresh,
                    user.Email,
                    isSuccess: false,
                    failureReason: AuthFailureReason.RefreshTokenReused,
                    userId: user.Id,
                    details: "Reuse of revoked token. All sessions revoked.");
            }
            else
            {
                await _auditService.LogAuthEventAsync
                    (SecurityEvents.TokenRefresh,
                    user.Email,
                    isSuccess: false,
                    failureReason: AuthFailureReason.RefreshTokenExpired,
                    userId: user.Id);
            }
            return null;
        }
        
        refreshToken.RevokedOn = DateTime.UtcNow;

        var userRoles = await _userManager.GetRolesAsync(user);
        var (newToken, expiresIn) = _jwtService.GenerateJwtToken(user, userRoles);

        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = refreshTokenExpiration
        });
        var cutoff = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenCutoffDays);
        user.RefreshTokens.RemoveAll(t => !t.IsActive && (t.RevokedOn ?? t.ExpiresOn) < cutoff);
        await _userManager.UpdateAsync(user);
        await _auditService.LogAuthEventAsync
            (SecurityEvents.TokenRefresh,
            user.Email,
            true,
            null,
            user.Id);
        return new AuthResponse
     (
         user.Id,
         user.Email ?? string.Empty,
         user.UserName ?? string.Empty,
         user.FirstName,
         user.LastName,
         newToken,
         expiresIn,
         newRefreshToken,
         refreshTokenExpiration
     );
    }
    public async Task<bool> RevokeTokenAsync(RevokeRefresheTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace( request.RefreshToken))
            return false;

        var user = await _userManager.Users
            .Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync((x => x.RefreshTokens.Any(t => t.Token == request.RefreshToken)), cancellationToken);
        if (user is null)
        {
                await _auditService.LogAuthEventAsync
                    (SecurityEvents.TokenRevoke,
                    null,
                    false,
                    AuthFailureReason.RefreshTokenNotFound,
                    null);
                
            
            return false;//لو وصل هنا يبقي في خطر او مشكلة
        }

        var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == request.RefreshToken);

        if (refreshToken is null)
        {
            await _auditService.LogAuthEventAsync
           (SecurityEvents.TokenRevoke,
           user.Email,
           false,
           AuthFailureReason.RefreshTokenNotFound,
           user.Id);
            return false;
        }
        if(!refreshToken.IsActive)
        {
            await _auditService.LogAuthEventAsync
          (SecurityEvents.TokenRevoke,
          user.Email,
          false,
          AuthFailureReason.RefreshTokenReused,
          user.Id);
            return false;
        }
        refreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        await _auditService.LogAuthEventAsync
            (SecurityEvents.TokenRevoke,
            user.Email,
            true,
            null,
            user.Id);
        return true;
    }
}