using Slot.Common.Traits;

namespace Slot.Common.Models;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public string NowIso => Now.ToString("O");
    public string UtcNowIso => UtcNow.ToString("O");
}