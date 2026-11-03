using System.Diagnostics;
namespace Bookify.Services;
public class SecurityAuditService(
    ApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SecurityAuditService> logger) : ISecurityAuditService
{
    public async Task LogAuthEventAsync
                                      (string eventType,
                                      string? attemptedIdentifier,
                                      bool isSuccess,
                                      AuthFailureReason? failureReason = null,
                                      string? userId = null,
                                      string? details = null)
    {
        var context = httpContextAccessor.HttpContext;
        var request = context?.Request;
        var ipAddress = context?.Connection.RemoteIpAddress?.ToString();
        var userAgent = request?.Headers.UserAgent.ToString();
        var clientBrowser = request?.Headers["Sec-CH-UA"].ToString();
        var clientPlatform = request?.Headers["Sec-CH-UA-Platform"].ToString();
        bool? isMobile = null;
        var mobileHint = request?.Headers["Sec-CH-UA-Mobile"].ToString();
        if (!string.IsNullOrEmpty(mobileHint))
        {
            isMobile = mobileHint.Contains("?1");
        }
        if (string.IsNullOrEmpty(clientBrowser) && !string.IsNullOrEmpty(userAgent))
        {
            clientBrowser = userAgent.Length > 100 ? userAgent[..100] : userAgent;
            isMobile = userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone");
            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) clientPlatform = "iOS";
            else if (userAgent.Contains("HarmonyOS") || userAgent.Contains("OpenHarmony")) clientPlatform = "HarmonyOS";
            else if (userAgent.Contains("Android")) clientPlatform = "Android";
            else if (userAgent.Contains("Windows")) clientPlatform = "Windows";
            else if (userAgent.Contains("Mac OS")) clientPlatform = "macOS";
            else if (userAgent.Contains("Linux")) clientPlatform = "Linux";
            else clientPlatform = "Other";
        }
        if (!string.IsNullOrEmpty(userAgent))
        {
            if (userAgent.Contains("FBAN")
                || userAgent.Contains("FBAV")
                || userAgent.Contains("FB_IAB"))
                clientBrowser = "Facebook In-App Browser";

            else if (userAgent.Contains("LinkedInApp"))
                clientBrowser = "LinkedIn In-App Browser";

            else if (userAgent.Contains("Instagram"))
                clientBrowser = "Instagram In-App Browser";

            else if (userAgent.Contains("TikTok")
                || userAgent.Contains("BytedanceWebview") ||
                userAgent.Contains("musical_ly"))
                clientBrowser = "TikTok In-App Browser";

            else if (userAgent.Contains("wv)")
                || userAgent.Contains("WebView"))
                clientBrowser = "Generic In-App WebView";
            if (string.IsNullOrWhiteSpace(clientBrowser)) clientBrowser = "Unknown";
            if (string.IsNullOrWhiteSpace(clientPlatform)) clientPlatform = "Unknown";
        }
        var auditLog = new SecurityAuditLog
        {
            EventType = eventType,
            IsSuccess = isSuccess,
            FailureReason = failureReason,
            Severity = DetermineSeverity(isSuccess, failureReason),
            UserId = userId,
            AttemptedIdentifier = attemptedIdentifier,
            IpAddress = ipAddress,
            HttpMethod = request?.Method,
            Path = request?.Path.ToString(),
            TraceId = Activity.Current?.Id ?? context?.TraceIdentifier,
            UserAgent = userAgent,
            ClientBrowser = clientBrowser?.Replace("\"", ""),
            ClientPlatform = clientPlatform?.Replace("\"", ""),
            IsMobile = isMobile,
            Details = details
        };
        try {
            dbContext.SecurityAuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            dbContext.Entry(auditLog).State = EntityState.Detached;
            logger.LogError(ex,
        "Failed to write security audit. Event: {EventType}, Identifier: {Identifier}, IP: {Ip}",
        eventType, attemptedIdentifier, ipAddress);
        }
    }
    private static AuditSeverity DetermineSeverity(bool isSuccess, AuthFailureReason? reason)
    {
        if (isSuccess) return AuditSeverity.Info;

        if (   reason == AuthFailureReason.RefreshTokenReused
            || reason == AuthFailureReason.InvalidAccessToken
            || reason == AuthFailureReason.LockedOut
            )
            return AuditSeverity.Critical;
        return AuditSeverity.Warning;
    }
}
