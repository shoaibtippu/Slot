using System.Security.Claims;
using Slot.Common.Models;
using Slot.Common.Traits;

namespace Slot.Host;

public class SessionInfoProvider(IHttpContextAccessor httpContextAccessor) : ISessionInfoProvider
{
    public UserInfo GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;

        var identityId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var email = user?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var userIdStr = user?.FindFirstValue("userId");
        var userId = userIdStr is not null ? Guid.Parse(userIdStr) : Guid.Empty;
        var roles = user?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                    ?? new List<string>();

        return new UserInfo(
            UserId: 0,
            ClientId: 0,
            TenantId: Guid.Empty,
            HasMultiTenantAccess: false,
            OrganizationId: 0,
            Roles: roles);
    }
}