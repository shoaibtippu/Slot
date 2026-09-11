using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Slot.Common.Models;

namespace Slot.Adapters.PostgreSql.Contexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ISessionInfoProvider sessionInfoProvider)
    : IdentityDbContext<IdentityUser>(options)
{
    private UserInfo? CurrentUser => sessionInfoProvider.GetCurrentUser();

    public new DbSet<User> Users => Set<User>();
    public DbSet<Ground> Grounds => Set<Ground>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<GroundAvailability> GroundAvailabilities => Set<GroundAvailability>();
    public DbSet<GroundImage> GroundImages => Set<GroundImage>();
    public DbSet<GroundSchedule> GroundSchedules => Set<GroundSchedule>();
    public DbSet<GroundSport> GroundSports => Set<GroundSport>();
    public DbSet<GroundTransaction> GroundTransactions => Set<GroundTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewReply> ReviewReplies => Set<ReviewReply>();
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<FavoriteGround> FavoriteGrounds => Set<FavoriteGround>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(IDeletionAuditedEntity).IsAssignableFrom(clrType))
            {
                var parameter = Expression.Parameter(clrType, "e");
                var deletedAtProp = Expression.Property(parameter, nameof(IDeletionAuditedEntity.DeletedAt));
                var notDeleted = Expression.Equal(deletedAtProp, Expression.Constant(null, typeof(DateTime?)));
                var lambda = Expression.Lambda(notDeleted, parameter);
                builder.Entity(clrType).HasQueryFilter(lambda);
            }
            else if (typeof(ISoftDeleted).IsAssignableFrom(clrType))
            {
                var parameter = Expression.Parameter(clrType, "e");
                var deletedProp = Expression.Property(parameter, nameof(ISoftDeleted.Deleted));
                var notDeleted = Expression.Equal(deletedProp, Expression.Constant(false));
                var lambda = Expression.Lambda(notDeleted, parameter);
                builder.Entity(clrType).HasQueryFilter(lambda);
            }

            if (typeof(IModificationAuditedEntity).IsAssignableFrom(clrType))
            {
                builder.Entity(clrType)
                    .Property(nameof(IModificationAuditedEntity.ModifiedAt))
                    .IsConcurrencyToken();

                if (typeof(IDeletionAuditedEntity).IsAssignableFrom(clrType))
                {
                    builder.Entity(clrType)
                        .Property(nameof(IDeletionAuditedEntity.DeletedAt))
                        .IsConcurrencyToken();
                }
            }
        }

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        if (CurrentUser is null) return;

        foreach (var entry in ChangeTracker.Entries())
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                if (entity is ICreationAuditedEntity creation)
                {
                    creation.CreatedBy = CurrentUser.UserId;
                    creation.CreatedAt = DateTime.UtcNow;
                }
                if (entity is IModificationAuditedEntity modOnCreate)
                {
                    modOnCreate.ModifiedBy = CurrentUser.UserId;
                    modOnCreate.ModifiedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified && entity is IModificationAuditedEntity modification)
            {
                modification.ModifiedBy = CurrentUser.UserId;
                modification.ModifiedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Deleted)
            {
                if (entity is IDeletionAuditedEntity deletion)
                {
                    entry.State = EntityState.Modified;
                    deletion.DeletedBy = CurrentUser.UserId;
                    deletion.DeletedAt = DateTime.UtcNow;
                    if (entity is ISoftDeleted soft) soft.Deleted = true;
                }
                else if (entity is ISoftDeleted softOnly)
                {
                    entry.State = EntityState.Modified;
                    softOnly.Deleted = true;
                }
            }
        }
    }
}