namespace Slot.Common.Models.Traits
{
    public interface ISoftDeleted
    {

        /// <summary>
        /// Gets or sets a value indicating whether the entity is soft-deleted.
        /// </summary>
        bool Deleted { get; set; }
    }
}
