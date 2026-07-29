namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class UserConfiguration : FullyAuditedEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        builder.HasKey(c => c.Id);
        builder.Property(u => u.ImageUrl).HasMaxLength(500).IsRequired(false);

        builder.HasOne(u => u.UserIdentity)
            .WithMany()
            .HasForeignKey(u => u.UserIdentityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.OwnedGrounds)
            .WithOne(g => g.Owner)
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}