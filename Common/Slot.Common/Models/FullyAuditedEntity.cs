using Slot.Common.Models.Traits;

namespace Slot.Common.Models
{
    public abstract class FullyAuditedEntity<TKey> : AuditedEntity<TKey>, IDeletionAuditedEntity
        where TKey : notnull
    {
        /// <inheritdoc />
        public virtual long? DeletedBy { get; set; }

        /// <inheritdoc />
        public virtual DateTime? DeletedAt { get; set; }

        /// <inheritdoc />
        public virtual bool Deleted { get; set; }
    }
}