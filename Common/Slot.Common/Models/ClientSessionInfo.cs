namespace Slot.Common.Models;

/// <param name="ClientId"> Id of the calling app/client. </param>
/// <param name="TenantId"> TenantId of the calling app/client. </param>
public record ClientInfo(int ClientId, Guid TenantId);