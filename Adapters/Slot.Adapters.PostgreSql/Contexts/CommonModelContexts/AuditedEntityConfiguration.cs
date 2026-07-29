namespace Slot.Adapters.PostgreSql.Contexts.CommonModelContexts;

public abstract class AuditedEntityConfiguration<TEntity> : EntityConfiguration<TEntity>
    where TEntity : class, IEntity<Guid>, ICreationAuditedEntity, IModificationAuditedEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.CreatedBy)
            .IsRequired(false);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ModifiedBy)
            .IsRequired(false);

        builder.Property(e => e.ModifiedAt)
            .IsRequired();
    }
}