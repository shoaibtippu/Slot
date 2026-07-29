using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Slot.Common.EF.Persistence;
using Slot.Common.Models;
using Slot.Common.Models.Traits;
using Slot.Common.Traits;

namespace Slot.Common.EF.Repositories;

public class BaseRepository<TEntity, TKey>(BaseFilterableDbContext<TKey> dbContext, ISessionInfoProvider sessionInfoProvider)
        where TEntity : class, IEntity<TKey>
        where TKey : notnull
{
    protected readonly BaseFilterableDbContext<TKey> DbContext = dbContext;
    private readonly UserInfo _sessionInfo = sessionInfoProvider.GetCurrentUser();
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    protected IQueryable<TEntity> TenantFilteredDbSet
    {
        get
        {
            var predicate = RestrictTenantAccess();
            return DbSet.Where(predicate);
        }
    }

    public IQueryable<TEntity> GetQueryable() => TenantFilteredDbSet;

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? throw new ArgumentNullException(nameof(predicate))
            : await DbSet
                .AsNoTracking()
                .AnyAsync(predicate, cancellationToken);
    }

    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        try
        {
            await DbSet.AddAsync(entity);
            await DbContext.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create entity of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task CreateRangeAsync(IEnumerable<TEntity> entities)
    {
        try
        {
            await DbSet.AddRangeAsync(entities);
            await DbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create entities of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            var entity = await TenantFilteredDbSet.FirstOrDefaultAsync(predicate);
            return entity ?? throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} not found.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to find entity of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        try
        {
            return await TenantFilteredDbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve entities of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<PagedResult<TEntity>> GetAllAsync(
        int? pageNumber,
        int? pageSize,
        List<(string PropertyName, bool IsAscending)>? sortFields = null,
        string? search = null,
        params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includeProperties)
    {
        try
        {
            var query = includeProperties.Aggregate(TenantFilteredDbSet.AsQueryable(), (current, include) => include(current));

            if (!string.IsNullOrEmpty(search))
            {
                var searchTerm = $"%{search}%";
                var stringProperties = typeof(TEntity)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType == typeof(string))
                    .ToList();

                if (stringProperties.Any())
                {
                    var parameter = Expression.Parameter(typeof(TEntity), "x");
                    Expression? body = null;

                    foreach (var prop in stringProperties)
                    {
                        var propAccess = Expression.Property(parameter, prop);
                        var like = Expression.Call(
                            typeof(DbFunctionsExtensions),
                            nameof(DbFunctionsExtensions.Like),
                            Type.EmptyTypes,
                            Expression.Constant(Microsoft.EntityFrameworkCore.EF.Functions),
                            propAccess,
                            Expression.Constant(searchTerm)
                        );
                        body = body == null ? like : Expression.OrElse(body, like);
                    }

                    if (body is not null)
                    {
                        var searchExpression = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
                        query = query.Where(searchExpression);
                    }
                }
            }

            if (sortFields is not null && sortFields.Any())
            {
                IOrderedQueryable<TEntity>? orderedQuery = null;

                foreach (var (propertyName, isAscending) in sortFields)
                {
                    var property = typeof(TEntity).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property is null)
                        throw new InvalidOperationException($"Invalid sort field: {propertyName}");

                    var parameter = Expression.Parameter(typeof(TEntity), "x");
                    var propertyAccess = Expression.Property(parameter, property);
                    var lambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(propertyAccess, typeof(object)), parameter);

                    if (orderedQuery == null)
                    {
                        orderedQuery = isAscending
                            ? query.OrderBy(lambda)
                            : query.OrderByDescending(lambda);
                    }
                    else
                    {
                        orderedQuery = isAscending
                            ? orderedQuery.ThenBy(lambda)
                            : orderedQuery.ThenByDescending(lambda);
                    }
                }

                if (orderedQuery != null)
                    query = orderedQuery;
            }

            var totalCount = await query.CountAsync();
            var actualPageNumber = pageNumber ?? 1;
            var actualPageSize = pageSize ?? totalCount;

            List<TEntity> data;
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                data = await query
                    .Skip((actualPageNumber - 1) * actualPageSize)
                    .Take(actualPageSize)
                    .ToListAsync();
            }
            else
            {
                data = await query.ToListAsync();
            }

            return PagedResult<TEntity>.Success(data, totalCount, actualPageNumber, actualPageSize);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve entities of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<TEntity> GetByIdAsync(TKey id)
    {
        if (id is null || id.Equals(default(TKey)))
            throw new ArgumentException($"Invalid Id provided for entity of type {typeof(TEntity).Name}.", nameof(id));

        var entity = await TenantFilteredDbSet.FirstOrDefaultAsync(e => e.Id.Equals(id));

        return entity ?? throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with Id {id} was not found.");
    }

    public async Task<TEntity> GetByIdAsync(TKey id, params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includeProperties)
    {
        if (id is null || id.Equals(default(TKey)))
            throw new ArgumentException($"Invalid Id provided for entity of type {typeof(TEntity).Name}.", nameof(id));

        var query = includeProperties.Aggregate(TenantFilteredDbSet.AsQueryable(), (current, include) => include(current));
        var entity = await query.FirstOrDefaultAsync(e => e.Id.Equals(id));

        return entity ?? throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with Id {id} was not found.");
    }

    public async Task<IReadOnlyList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate), $"Predicate cannot be null for entity type {typeof(TEntity).Name}.");

        try
        {
            var entities = await TenantFilteredDbSet.Where(predicate).ToListAsync();
            return entities;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to find entities of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<TEntity>> FindAllWithIncludeAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate), $"Predicate cannot be null for entity type {typeof(TEntity).Name}.");

        try
        {
            IQueryable<TEntity> query = TenantFilteredDbSet.Where(predicate);

            if (include is not null)
                query = include(query);

            var entities = await query.ToListAsync();
            return entities;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to find entities of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<TEntity> UpdateByIdAsync(TKey id, TEntity entity, DateTime? rowVersion)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity), $"Entity cannot be null for type {typeof(TEntity).Name}.");

        try
        {
            var existing = await TenantFilteredDbSet.FirstOrDefaultAsync(e => e.Id.Equals(id));
            if (existing == null)
                throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with Id {id} was not found.");

            var entry = DbContext.Entry(existing);

            if (rowVersion.HasValue)
            {
                var utcRowVersion = rowVersion.Value.Kind == DateTimeKind.Utc
                    ? rowVersion.Value
                    : rowVersion.Value.ToUniversalTime();
                var rowVersionProp = entry.Property("ModifiedAt");
                rowVersionProp.OriginalValue = utcRowVersion;
            }

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                var newValue = typeof(TEntity).GetProperty(property.Metadata.Name)?.GetValue(entity);
                property.CurrentValue = newValue;
            }

            await DbContext.SaveChangesAsync();
            return existing;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "Concurrency conflict: The entity was modified by another user. Please reload and try again.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update entity of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<TEntity> DeleteAsync(TKey id, DateTime? rowVersion)
    {
        try
        {
            var entity = await TenantFilteredDbSet.FirstOrDefaultAsync(e => e.Id.Equals(id));
            if (entity == null)
                throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with Id {id} was not found.");

            var entry = DbContext.Entry(entity);

            if (rowVersion.HasValue)
            {
                var utcRowVersion = rowVersion.Value.Kind == DateTimeKind.Utc
                    ? rowVersion.Value
                    : rowVersion.Value.ToUniversalTime();

                var modifiedAtProp = entry.Property("ModifiedAt");
                modifiedAtProp.OriginalValue = utcRowVersion;
            }

            DbSet.Remove(entity);
            await DbContext.SaveChangesAsync();

            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "Concurrency conflict: The entity was modified or deleted by another user. Please reload and try again.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete entity of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<TEntity>> GetByPropertyAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        TProperty value)
    {
        try
        {
            var parameter = propertyExpression.Parameters[0];
            var property = propertyExpression.Body;
            var constant = Expression.Constant(value, typeof(TProperty));

            var equality = CreateEqualityExpression(property, constant);

            var isDeletedCondition = CreateIsDeletedCondition(parameter);

            var combinedExpression = Expression.AndAlso(equality, isDeletedCondition);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);

            var entities = await TenantFilteredDbSet.Where(lambda).ToListAsync();
            return entities;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve entities: {ex.Message}", ex);
        }
    }

    private static Expression CreateEqualityExpression(Expression property, Expression constant)
    {
        if (Nullable.GetUnderlyingType(property.Type) != null)
        {
            var hasValue = Expression.Property(property, "HasValue");
            var valueProperty = Expression.Property(property, "Value");
            return Expression.AndAlso(
                hasValue,
                Expression.Equal(valueProperty, Expression.Convert(constant, valueProperty.Type))
            );
        }
        else
        {
            return Expression.Equal(property, constant);
        }
    }

    private static Expression CreateIsDeletedCondition(ParameterExpression parameter)
    {
        var isDeletedProperty = Expression.Property(parameter, "IsDeleted");
        return Expression.Equal(isDeletedProperty, Expression.Constant(false));
    }

    public async Task<IReadOnlyList<TEntity>> FindAllAsyncInclude(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        if (predicate is null)
            throw new ArgumentException("Predicate cannot be null.", nameof(predicate));

        IQueryable<TEntity> query = TenantFilteredDbSet.Where(predicate);

        if (include is not null)
        {
            query = include(query);
        }

        try
        {
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve entities of type {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    public async Task<Result<TEntity>> PermanentDeleteAsync(TKey id)
    {
        if (id is null || id.Equals(default(TKey)))
            throw new ArgumentException("Invalid Id provided.", nameof(id));

        var entity = await GetByIdAsync(id);
        if (entity is null)
            throw new KeyNotFoundException($"Entity of type {typeof(TEntity).Name} with Id {id} was not found.");

        try
        {
            DbSet.Remove(entity);
            await DbContext.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Failed to delete entity due to database constraint: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete entity: {ex.Message}", ex);
        }
    }

    protected virtual Expression<Func<TEntity, bool>> RestrictTenantAccess()
    {
        Expression<Func<TEntity, bool>> predicate = e => true;

        if (_sessionInfo.HasMultiTenantAccess)
            return predicate;

        // Check if TEntity implements ITenant
        if (typeof(IMustHaveTenant).IsAssignableFrom(typeof(TEntity)) ||
            typeof(IMayHaveTenant).IsAssignableFrom(typeof(TEntity)))
        {
            // Create a parameter expression (same as the one used in the predicate)
            var parameter = predicate.Parameters[0];
            // Create an expression for "e.TenantId == sessionInfo.TenantId"
            var tenantCondition = Expression.Equal(
                Expression.Property(parameter, nameof(IMustHaveTenant.OrganizationId)),
                Expression.Constant(_sessionInfo.OrganizationId)
            );
            if (typeof(IMayHaveTenant).IsAssignableFrom(typeof(TEntity)))
            {
                // If TEntity implements IMayHaveTenant, allow null TenantId
                var tenantIdProperty = Expression.Property(parameter, nameof(IMayHaveTenant.OrganizationId));
                var isTenantIdNull = Expression.Equal(tenantIdProperty, Expression.Constant(null, typeof(int?)));
                tenantCondition = Expression.OrElse(tenantCondition, isTenantIdNull);
            }
            // Combine the existing predicate with the "tenantCondition" condition
            var combinedExpression = Expression.AndAlso(predicate.Body, tenantCondition);
            // Wrap the combined expression in a lambda
            return Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
        }

        return predicate;
    }
}