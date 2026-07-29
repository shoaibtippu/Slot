namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class FavoriteGroundConfiguration : IEntityTypeConfiguration<FavoriteGround>
{
    public void Configure(EntityTypeBuilder<FavoriteGround> builder)
    {
        builder.HasKey(fg => new { fg.UserId, fg.GroundId });

        builder.HasOne(fg => fg.User)
            .WithMany()
            .HasForeignKey(fg => fg.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fg => fg.Ground)
            .WithMany()
            .HasForeignKey(fg => fg.GroundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}