namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class MessageConfiguration : FullyAuditedEntityConfiguration<Message>
{
    public override void Configure(EntityTypeBuilder<Message> builder)
    {
        base.Configure(builder);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Text).HasMaxLength(4000).IsRequired();
        builder.Property(m => m.IsRead).IsRequired().HasDefaultValue(false);

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}