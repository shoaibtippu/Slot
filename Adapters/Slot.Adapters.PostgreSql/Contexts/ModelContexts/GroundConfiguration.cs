namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class GroundConfiguration : FullyAuditedEntityConfiguration<Ground>
{
    public override void Configure(EntityTypeBuilder<Ground> builder)
    {
        base.Configure(builder);
        builder.HasKey(c => c.Id);

        builder.Property(g => g.Name).HasMaxLength(200).IsRequired(false);
        builder.Property(g => g.Description).HasMaxLength(2000).IsRequired(false);
        builder.Property(g => g.Address).HasMaxLength(500).IsRequired(false);
        builder.Property(g => g.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(g => g.AlternatePhoneNumber).HasMaxLength(20).IsRequired(false);
        builder.Property(g => g.Latitude).HasPrecision(10, 7);
        builder.Property(g => g.Longitude).HasPrecision(10, 7);
        builder.Property(g => g.HourlyRate).HasPrecision(18, 2);
        builder.Property(g => g.AdvancePercentage).HasPrecision(5, 2);
        builder.Property(g => g.AverageRating).HasPrecision(3, 2);
    }
}