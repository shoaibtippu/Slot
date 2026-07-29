namespace Slot.Adapters.PostgreSql.Contexts.CommonModelContexts;

public abstract class FullyAuditedEntityConfiguration<TEntity> : AuditedEntityConfiguration<TEntity>
    where TEntity : class, IEntity<Guid>, ICreationAuditedEntity, IModificationAuditedEntity, IDeletionAuditedEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.DeletedBy)
            .IsRequired(false);

        builder.Property(e => e.DeletedAt)
            .IsRequired(false);

        builder.Property(e => e.Deleted)
            .IsRequired();
    }
}