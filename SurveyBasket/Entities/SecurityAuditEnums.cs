namespace Bookify.Entities;

public enum AuditSeverity
{
    Info,
    Warning,
    Critical
}

public enum AuthFailureReason
{   
    UserNotFound,
    InvalidPassword,
    LockedOut,
    InvalidAccessToken,
    RefreshTokenNotFound,
    RefreshTokenReused,
    RefreshTokenExpired,
    RegistrationFailed
}

public static class SecurityEvents
{
    public const string Register = "auth.register";
    public const string Login = "auth.login";
    public const string TokenRefresh = "auth.token.refresh";
    public const string TokenRevoke = "auth.token.revoke";
    public const string AccessDenied = "authz.access_denied";
}