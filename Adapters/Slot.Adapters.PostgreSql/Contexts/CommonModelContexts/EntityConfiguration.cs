namespace Slot.Adapters.PostgreSql.Contexts.CommonModelContexts;

public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
where TEntity : class, IEntity<Guid>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(c => c.Id);
    }
}