namespace Bookify.Entities;

public sealed class SecurityAuditLog
{
    public long Id { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public AuthFailureReason? FailureReason { get; set; }
    public AuditSeverity Severity { get; set; } = AuditSeverity.Info;
    public string? UserId { get; set; }
    public string? AttemptedIdentifier { get; set; }
    public string? IpAddress { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public string? TraceId { get; set; }
    public string? UserAgent { get; set; }
    public string? ClientBrowser { get; set; }
    public string? ClientPlatform { get; set; }
    public bool? IsMobile { get; set; }
    public string? Details { get; set; }
}