namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class ReviewConfiguration : FullyAuditedEntityConfiguration<Review>
{
    public override void Configure(EntityTypeBuilder<Review> builder)
    {
        base.Configure(builder);
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(1000).IsRequired(false);

        // One review per booking
        builder.HasIndex(r => r.BookingId).IsUnique();

        builder.HasOne(r => r.Ground)
            .WithMany(g => g.Reviews)
            .HasForeignKey(r => r.GroundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Booking)
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
