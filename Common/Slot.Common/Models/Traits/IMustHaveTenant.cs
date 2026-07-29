namespace Slot.Common.Models.Traits;

public interface IMustHaveTenant
{
    /// <summary>
    /// Gets or sets the identifier of the tenant to which the entity belongs.
    /// </summary>
    int OrganizationId { get; set; }
}