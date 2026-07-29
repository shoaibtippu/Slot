using Slot.Common.Models.Traits;

namespace Slot.Common.Models
{
    public abstract class Entity<TKey> : IEntity<TKey>
        where TKey : notnull
    {
        /// <inheritdoc />
        public virtual required TKey Id { get; set; }

        /// <inheritdoc />
        public virtual bool IsTransient()
        {
            if (EqualityComparer<TKey>.Default.Equals(Id, default))
                return true;

            if (typeof(TKey) == typeof(int))
            {
                return Convert.ToInt32(Id) <= 0;
            }

            if (typeof(TKey) == typeof(long))
            {
                return Convert.ToInt64(Id) <= 0;
            }

            return false;
        }
    }
}