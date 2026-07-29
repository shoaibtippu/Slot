namespace Slot.Common.Traits;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    string NowIso { get; }
    string UtcNowIso { get; }
}