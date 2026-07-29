namespace Slot.Adapters.PostgreSql.Contexts.ModelContexts;

public class SportConfiguration :FullyAuditedEntityConfiguration<Sport>
{
    public override void Configure(EntityTypeBuilder<Sport> builder)
    {
        base.Configure(builder);
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.IconUrl).HasMaxLength(500).IsRequired(false);

        builder.HasIndex(s => s.Name).IsUnique();
    }
}