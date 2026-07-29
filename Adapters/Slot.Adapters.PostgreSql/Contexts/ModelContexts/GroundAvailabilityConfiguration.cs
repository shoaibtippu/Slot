namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class GroundAvailabilityConfiguration : FullyAuditedEntityConfiguration<GroundAvailability>
{
    public override void Configure(EntityTypeBuilder<GroundAvailability> builder)
    {
        base.Configure(builder);
        builder.HasKey(ga => ga.Id);

        builder.Property(ga => ga.IsBlocked).IsRequired().HasDefaultValue(false);

        builder.HasIndex(ga => new { ga.GroundId, ga.Date });

        // FK to Ground — no nav property on Ground side so use the FK directly
        builder.HasOne<Ground>()
            .WithMany()
            .HasForeignKey(ga => ga.GroundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}