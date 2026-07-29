namespace Slot.Common.Models.Traits
{
    public interface IDeletionAuditedEntity: ISoftDeleted
    {
        /// <summary>
        /// Gets or sets the identifier of the user who deleted the entity.
        /// </summary>
        long? DeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was deleted.
        /// </summary>
        DateTime? DeletedAt { get; set; }
    }
}
