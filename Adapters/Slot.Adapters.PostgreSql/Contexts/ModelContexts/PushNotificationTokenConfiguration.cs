namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class PushNotificationTokenConfiguration : FullyAuditedEntityConfiguration<PushNotificationToken>
{
    public override void Configure(EntityTypeBuilder<PushNotificationToken> builder)
    {
        base.Configure(builder);
        builder.HasKey(e => e.Id);

        builder.Property(t => t.Token).HasMaxLength(512).IsRequired();
        builder.Property(t => t.Platform).HasMaxLength(50);

        builder.HasIndex(t => new { t.UserId, t.Token }).IsUnique();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
