using Slot.Common.Models;

namespace Slot.Common.Traits;

public interface ISessionInfoProvider
{
    UserInfo? GetCurrentUser();
}