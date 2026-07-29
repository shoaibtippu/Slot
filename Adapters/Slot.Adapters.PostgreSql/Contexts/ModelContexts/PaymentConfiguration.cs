namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class PaymentConfiguration : FullyAuditedEntityConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Method).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.TransactionReference).HasMaxLength(200).IsRequired(false);
        builder.Property(p => p.PaidAt).IsRequired(false);

        builder.HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}