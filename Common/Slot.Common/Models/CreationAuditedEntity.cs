using Slot.Common.Models.Traits;

namespace Slot.Common.Models;

public class CreationAuditedEntity : ICreationAuditedEntity
{
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}