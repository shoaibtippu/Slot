using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Slot.Common.Models;
using Slot.Common.Models.Traits;
using Slot.Common.Traits;

namespace Slot.Common.EF.Persistence;

public abstract class BaseFilterableDbContext<TKey>(
    DbContextOptions options,
    ISessionInfoProvider sessionInfoProvider)
    : DbContext(options)
    where TKey : notnull
{
    private UserInfo CurrentUser => sessionInfoProvider.GetCurrentUser();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Apply soft-delete/global filters
            if (typeof(IDeletionAuditedEntity).IsAssignableFrom(clrType))
            {
                // e => e.DeletedAt == null
                var parameter = Expression.Parameter(clrType, "e");
                var deletedAtProp = Expression.Property(parameter, nameof(IDeletionAuditedEntity.DeletedAt));
                var notDeleted = Expression.Equal(
                    deletedAtProp,
                    Expression.Constant(null, typeof(DateTime?))
                );
                var lambda = Expression.Lambda(notDeleted, parameter);
                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
            else if (typeof(ISoftDeleted).IsAssignableFrom(clrType))
            {
                // e => e.Deleted == false
                var parameter = Expression.Parameter(clrType, "e");
                var deletedProp = Expression.Property(parameter, nameof(ISoftDeleted.Deleted));
                var notDeleted = Expression.Equal(deletedProp, Expression.Constant(false));
                var lambda = Expression.Lambda(notDeleted, parameter);
                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }

            // Concurrency token for ModifiedAt
            if (typeof(IModificationAuditedEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType)
                    .Property(nameof(IModificationAuditedEntity.ModifiedAt))
                    .IsConcurrencyToken();

                // Concurrency token for DeletedAt when present
                if (typeof(IDeletionAuditedEntity).IsAssignableFrom(clrType))
                {
                    modelBuilder.Entity(clrType)
                        .Property(nameof(IDeletionAuditedEntity.DeletedAt))
                        .IsConcurrencyToken();
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateFields()
    {
        if (CurrentUser is null)
        {
            return;
        }

        var entries = ChangeTracker.Entries();
        foreach (var entry in entries)
        {
            var entity = entry.Entity;

            // This is part of the common project and does not contain concrete implementations, so we exclude it from Sonar analysis.
#pragma warning disable S1944
            if (entry.State == EntityState.Added)
            {
                if (entity is ICreationAuditedEntity creationAuditedEntity)
                {
                    // Set creation audit fields
                    creationAuditedEntity.CreatedBy = CurrentUser.UserId;
                    creationAuditedEntity.CreatedAt = DateTime.UtcNow;
                }

                // Initialize modification audit fields on creation so concurrency token has a valid value
                if (entity is IModificationAuditedEntity modificationOnCreate)
                {
                    modificationOnCreate.ModifiedBy = CurrentUser.UserId;
                    modificationOnCreate.ModifiedAt = DateTime.UtcNow; // prevent default 0001-01-01 causing concurrency mismatch
                }

                // Set tenant values centrally
                // For users with multitenant access, we do not set the tenant ID if it is already set by the calling code.
                if (entity is IMustHaveTenant mustHaveTenant &&
                    (!CurrentUser.HasMultiTenantAccess || mustHaveTenant.OrganizationId == default))
                {
                    mustHaveTenant.OrganizationId = CurrentUser.OrganizationId;
                }
                else if (entity is IMayHaveTenant mayHaveTenant &&
                         (!CurrentUser.HasMultiTenantAccess || mayHaveTenant.OrganizationId == default))
                {
                    mayHaveTenant.OrganizationId = CurrentUser.OrganizationId;
                }
            }
            else if (entry.State == EntityState.Modified &&
                     entity is IModificationAuditedEntity modificationAuditedEntity)
            {
                // Set modification audit fields
                modificationAuditedEntity.ModifiedBy = CurrentUser.UserId;
                modificationAuditedEntity.ModifiedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Deleted)
            {
                // Soft delete
                if (entity is IDeletionAuditedEntity deletionAudited)
                {
                    entry.State = EntityState.Modified;
                    deletionAudited.DeletedBy = CurrentUser.UserId;
                    deletionAudited.DeletedAt = DateTime.UtcNow;

                    // Optional: keep legacy bool in sync if present
                    if (entity is ISoftDeleted softDeleted)
                        softDeleted.Deleted = true;
                }
                else if (entity is ISoftDeleted softDeletedOnly)
                {
                    // Fallback for entities not yet upgraded to IDeletionAuditedEntity
                    entry.State = EntityState.Modified;
                    softDeletedOnly.Deleted = true;
                }
            }
#pragma warning restore S1944
        }
    }
}