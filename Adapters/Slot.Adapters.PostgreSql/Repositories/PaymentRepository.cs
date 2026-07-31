namespace Slot.Adapters.PostgreSql.Repositories;

public class PaymentRepository(ApplicationDbContext db) : IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await db.Payments
            .AsNoTracking()
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Payment?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Payment?> FindByBookingAndTransactionAsync(Guid bookingId, string transactionReference, CancellationToken ct = default)
    {
        return await db.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId && p.TransactionReference == transactionReference, ct);
    }

    public async Task CreateAsync(Payment payment, CancellationToken ct = default)
    {
        await db.Payments.AddAsync(payment, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Update(payment);
        await db.SaveChangesAsync(ct);
    }
}
