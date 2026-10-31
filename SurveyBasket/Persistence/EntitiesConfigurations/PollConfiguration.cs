namespace Bookify.Persistence.EntitiesConfigurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{

    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasIndex(e => e.Title).IsUnique();
        builder.Property(e=>e.Title).HasMaxLength(100).IsRequired();
        builder.Property(e=>e.Summary).HasMaxLength(1500).IsRequired();
        builder.Property(e => e.IsPublished).IsRequired();
        builder.Property(e => e.StartsAt).IsRequired();
        builder.Property(e => e.EndsAt).IsRequired();
    }
}
