using Slot.Common.Models.Traits;

namespace Slot.Common.Models
{
    public abstract class AuditedEntity<TKey> : Entity<TKey>, ICreationAuditedEntity, IModificationAuditedEntity
        where TKey : notnull
    {
        /// <inheritdoc />
        public virtual long? CreatedBy { get; set; }

        /// <inheritdoc />
        public virtual DateTime CreatedAt { get; set; }

        /// <inheritdoc />
        public virtual long? ModifiedBy { get; set; }

        /// <inheritdoc />
        public virtual DateTime ModifiedAt { get; set; }
    }
}