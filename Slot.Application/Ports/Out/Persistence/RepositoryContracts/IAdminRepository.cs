namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IAdminRepository
{
    Task<IReadOnlyList<(User Profile, IdentityUser Identity, IReadOnlyList<string> Roles, bool IsActive)>> GetUsersAsync(CancellationToken ct = default);
    Task<(User Profile, IdentityUser Identity, IReadOnlyList<string> Roles, bool IsActive)?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task UpdateUserRolesAsync(string identityUserId, IReadOnlyList<string> roles, CancellationToken ct = default);
    Task DeactivateUserAsync(string identityUserId, CancellationToken ct = default);
    Task<IReadOnlyList<Ground>> GetGroundsAsync(CancellationToken ct = default);
    Task DeleteGroundAsync(Ground ground, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetBookingsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetPaymentsAsync(CancellationToken ct = default);
    Task<int> GetUserCountAsync(CancellationToken ct = default);
    Task<int> GetGroundCountAsync(CancellationToken ct = default);
    Task<int> GetBookingCountAsync(CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    Task<decimal> GetPendingRevenueAsync(CancellationToken ct = default);
    Task<decimal> GetCompletedRevenueAsync(CancellationToken ct = default);
}
