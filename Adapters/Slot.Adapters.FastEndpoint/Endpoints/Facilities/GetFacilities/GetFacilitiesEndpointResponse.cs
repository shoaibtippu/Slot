using System.Collections.Generic;

namespace Slot.Adapters.FastEndpoint.Endpoints.Facilities.GetFacilities;

public class GetFacilitiesEndpointResponse
{
    public IEnumerable<FacilityDto> Data { get; set; } = new List<FacilityDto>();
    public string? Error { get; set; }
}

public class FacilityDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Rating { get; set; }
    public int TotalReviews { get; set; }
    public string[] Tags { get; set; } = System.Array.Empty<string>();
    public PricingDto Pricing { get; set; } = new PricingDto();
    public System.DateTime CreatedAt { get; set; }
}

public class PricingDto
{
    public int HourlyRate { get; set; }
    public string Currency { get; set; } = string.Empty;
}
