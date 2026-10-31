namespace Bookify.Services;

public interface ISecurityAuditService
{
    Task LogAuthEventAsync(
        string eventType,
        string? attemptedIdentifier,
        bool isSuccess,
        AuthFailureReason? failureReason = null,
        string? userId = null,
        string? details = null);
}
