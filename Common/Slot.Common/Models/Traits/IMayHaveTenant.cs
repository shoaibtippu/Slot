namespace Slot.Common.Models.Traits
{
    public interface IMayHaveTenant
    {
        /// <summary>
        /// Gets or sets the identifier of the tenant to which the entity may belong.
        /// </summary>
        int? OrganizationId { get; set; }
    }
}
