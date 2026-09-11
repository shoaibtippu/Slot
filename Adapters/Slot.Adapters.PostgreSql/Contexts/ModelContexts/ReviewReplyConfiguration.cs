namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class ReviewReplyConfiguration : AuditedEntityConfiguration<ReviewReply>
{
    public override void Configure(EntityTypeBuilder<ReviewReply> builder)
    {
        base.Configure(builder);
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Text).IsRequired().HasMaxLength(2000);

        builder.HasIndex(r => r.ReviewId).IsUnique();

        builder.HasOne(r => r.Review)
            .WithOne(rev => rev.Reply)
            .HasForeignKey<ReviewReply>(r => r.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Owner)
            .WithMany()
            .HasForeignKey(r => r.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
