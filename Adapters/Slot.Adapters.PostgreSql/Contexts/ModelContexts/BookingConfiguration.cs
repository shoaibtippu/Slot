namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class BookingConfiguration : FullyAuditedEntityConfiguration<Booking>
{
    public override void Configure(EntityTypeBuilder<Booking> builder)
    {
        base.Configure(builder);
        builder.HasKey(c => c.Id);

        builder.Property(b => b.PricePerHour).HasPrecision(18, 2);
        builder.Property(b => b.TotalAmount).HasPrecision(18, 2);
        builder.Property(b => b.AdvanceAmount).HasPrecision(18, 2);
        builder.Property(b => b.RemainingAmount).HasPrecision(18, 2);
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.Notes).HasMaxLength(1000).IsRequired(false);

        builder.HasOne(b => b.Ground)
            .WithMany(g => g.Bookings)
            .HasForeignKey(b => b.GroundId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}