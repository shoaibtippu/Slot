namespace Slot.Common.Models.Traits
{
    public interface IEntity<TKey>
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entity.
        /// </summary>
        TKey Id { get; set; }

        /// <summary>
        /// returns true if the entity is transient (i.e., not yet persisted to the database).
        /// </summary>
        /// <returns>
        /// True if the entity is transient; otherwise, false.
        /// </returns>
        bool IsTransient();
    }
}
