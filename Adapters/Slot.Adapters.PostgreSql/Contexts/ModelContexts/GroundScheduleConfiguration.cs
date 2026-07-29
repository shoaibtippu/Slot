namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class GroundScheduleConfiguration : FullyAuditedEntityConfiguration<GroundSchedule>
{
    public override void Configure(EntityTypeBuilder<GroundSchedule> builder)
    {
        base.Configure(builder);
        builder.HasKey(gs => gs.Id);

        builder.Property(gs => gs.DayOfWeek).IsRequired();
        builder.Property(gs => gs.IsClosed).IsRequired().HasDefaultValue(false);

        builder.HasOne(gs => gs.Ground)
            .WithMany(g => g.Schedules)
            .HasForeignKey(gs => gs.GroundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}