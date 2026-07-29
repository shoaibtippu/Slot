namespace Slot.Common.Models.Traits
{
    public interface IModificationAuditedEntity
    {
        /// <summary>
        /// Gets or sets the identifier of the user who last modified the entity.
        /// </summary>
        long? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was last modified.
        /// </summary>
        DateTime ModifiedAt { get; set; }
    }
}
