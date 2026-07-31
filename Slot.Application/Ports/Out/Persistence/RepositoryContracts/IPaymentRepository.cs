namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IPaymentRepository
{
    Task<IReadOnlyList<Payment>> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<Payment?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<Payment?> FindByBookingAndTransactionAsync(Guid bookingId, string transactionReference, CancellationToken ct = default);
    Task CreateAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
}
