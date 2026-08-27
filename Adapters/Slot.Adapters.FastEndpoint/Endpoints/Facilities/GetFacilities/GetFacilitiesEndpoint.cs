namespace Slot.Adapters.FastEndpoint.Endpoints.Facilities.GetFacilities;

[Authorize]
public class GetFacilitiesEndpoint(IFacilityService facilityService) : EndpointWithoutRequest<GetFacilitiesEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/facilities");
        Summary(s =>
        {
            s.Summary = "List of facilities for authenticated users.";
            s.Response<GetFacilitiesEndpointResponse>(200, "Facilities retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await facilityService.GetAllAsync(ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetFacilitiesEndpointResponse { Data = System.Array.Empty<FacilityDto>(), Error = result.Error?.Message }, result.Error?.Code ?? 500, ct);
            return;
        }

        var data = result.Value!.Select(f => new FacilityDto
        {
            Id = f.Id,
            Name = f.Name ?? string.Empty,
            Status = f.Status,
            ImageUrl = f.ImageUrl ?? string.Empty,
            Location = f.Location ?? string.Empty,
            Rating = (double)f.Rating,
            TotalReviews = f.TotalReviews,
            Tags = f.Tags.ToArray(),
            Pricing = new PricingDto { HourlyRate = f.HourlyRate, Currency = f.Currency },
            CreatedAt = f.CreatedAt
        }).ToList();

        await Send.OkAsync(new GetFacilitiesEndpointResponse { Data = data, Error = null }, ct);
    }
}