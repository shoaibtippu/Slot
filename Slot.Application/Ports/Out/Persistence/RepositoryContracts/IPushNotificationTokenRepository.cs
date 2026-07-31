namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IPushNotificationTokenRepository
{
    Task<PushNotificationToken?> FindAsync(Guid userId, string token, CancellationToken ct = default);
    Task CreateAsync(PushNotificationToken pushNotificationToken, CancellationToken ct = default);
    Task UpdateAsync(PushNotificationToken pushNotificationToken, CancellationToken ct = default);
    Task DeleteAsync(PushNotificationToken pushNotificationToken, CancellationToken ct = default);
}
