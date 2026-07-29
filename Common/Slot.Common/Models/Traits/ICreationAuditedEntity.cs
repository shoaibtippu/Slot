namespace Slot.Common.Models.Traits
{
    public interface ICreationAuditedEntity
    {
        /// <summary>
        /// Gets or sets the identifier of the user who created the entity.
        /// </summary>
        long? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        DateTime CreatedAt { get; set; }
    }
}