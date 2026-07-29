namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class GroundTransactionConfiguration : FullyAuditedEntityConfiguration<GroundTransaction>
{
    public override void Configure(EntityTypeBuilder<GroundTransaction> builder)
    {
        base.Configure(builder);
        builder.HasKey(gt => gt.Id);

        builder.Property(gt => gt.Amount).HasPrecision(18, 2);
        builder.Property(gt => gt.CommissionAmount).HasPrecision(18, 2);
        builder.Property(gt => gt.OwnerAmount).HasPrecision(18, 2);

        builder.HasOne(gt => gt.Ground)
            .WithMany()
            .HasForeignKey(gt => gt.GroundId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gt => gt.Booking)
            .WithMany()
            .HasForeignKey(gt => gt.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}