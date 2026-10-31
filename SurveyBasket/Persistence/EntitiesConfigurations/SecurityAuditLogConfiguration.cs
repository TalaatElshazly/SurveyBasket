using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bookify.Entities;
namespace Bookify.Persistence.EntitiesConfigurations;

public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("SecurityAuditLogs");
        builder.Property(x => x.EventType).HasMaxLength(64).IsRequired();
        ///////////
        builder.Property(x => x.FailureReason).HasConversion<string>().HasMaxLength(64);
        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(16);
        ///////////
        builder.Property(x => x.AttemptedIdentifier).HasMaxLength(256);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.HttpMethod).HasMaxLength(10);
        builder.Property(x => x.Path).HasMaxLength(256);
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.ClientBrowser).HasMaxLength(100);
        builder.Property(x => x.ClientPlatform).HasMaxLength(50);
        builder.Property(x => x.Details).HasMaxLength(2000);
        /////////
        builder.HasIndex(x => new { x.UserId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.IpAddress, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.EventType, x.IsSuccess, x.OccurredAtUtc });
    }
}