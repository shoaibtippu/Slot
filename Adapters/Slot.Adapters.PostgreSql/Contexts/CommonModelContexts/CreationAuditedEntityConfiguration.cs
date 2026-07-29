namespace Slot.Adapters.PostgreSql.Contexts.CommonModelContexts;

public abstract class CreationAuditedEntityConfiguration<TEntity> : EntityConfiguration<TEntity>
    where TEntity : class, IEntity<Guid>, ICreationAuditedEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.CreatedBy)
            .IsRequired(false);

        builder.Property(e => e.CreatedAt)
            .IsRequired();
    }
}