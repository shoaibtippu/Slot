namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class GroundImageConfiguration : FullyAuditedEntityConfiguration<GroundImage>
{
    public override void Configure(EntityTypeBuilder<GroundImage> builder)
    {
        base.Configure(builder);
        builder.HasKey(g => g.Id);

        builder.Property(gi => gi.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(gi => gi.DisplayOrder).IsRequired();

        builder.HasOne(gi => gi.Ground)
            .WithMany(g => g.Images)
            .HasForeignKey(gi => gi.GroundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}