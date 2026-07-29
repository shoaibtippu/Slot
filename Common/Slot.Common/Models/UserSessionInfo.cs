namespace Slot.Common.Models;

/// <param name="UserId"> Id of the calling user. </param>
/// <param name="TenantId"> TenantId of the calling user. </param>
/// <param name="HasMultiTenantAccess"> True only if the user is super admin. </param>
public record UserInfo(long UserId, int ClientId, Guid TenantId, bool HasMultiTenantAccess, int OrganizationId, IReadOnlyList<string> Roles) : ClientInfo(ClientId, TenantId);