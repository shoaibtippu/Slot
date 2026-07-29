namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class GroundSportConfiguration : IEntityTypeConfiguration<GroundSport>
{
    public void Configure(EntityTypeBuilder<GroundSport> builder)
    {
        builder.HasKey(gs => new { gs.GroundId, gs.SportId });

        builder.HasOne(gs => gs.Ground)
            .WithMany(g => g.Sports)
            .HasForeignKey(gs => gs.GroundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gs => gs.Sport)
            .WithMany(s => s.GroundSports)
            .HasForeignKey(gs => gs.SportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}