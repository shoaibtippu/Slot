namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class ConversationConfiguration : FullyAuditedEntityConfiguration<Conversation>
{
    public override void Configure(EntityTypeBuilder<Conversation> builder)
    {
        base.Configure(builder);
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.BookingId).IsUnique(); // one conversation per booking

        // BookingId FK — no nav on Booking side
        builder.HasOne<Booking>()
            .WithMany()
            .HasForeignKey(c => c.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // GroundOwner FK — must explicitly name nav property so EF wires Include(c => c.GroundOwner)
        builder.HasOne(c => c.GroundOwner)
            .WithMany()
            .HasForeignKey(c => c.GroundOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // User FK
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
