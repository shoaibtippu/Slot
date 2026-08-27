using Slot.Application.Ports.In.Facilities;
using Slot.Application.Ports.Out.Persistence.RepositoryContracts;

namespace Slot.Application.Services;

public class FacilityService(IGroundRepository groundRepository) : IFacilityService
{
    public async Task<Result<IReadOnlyList<FacilityResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var grounds = await groundRepository.GetAllAsync(new GroundListRequest(null, null, null, null, null, null, null), ct);

        var result = grounds.Select(g => new FacilityResponse(
            g.Id.ToString(),
            g.Name,
            g.Deleted ? "INACTIVE" : "ACTIVE",
            g.Images.FirstOrDefault()?.ImageUrl,
            g.Address,
            g.AverageRating,
            g.TotalReviews,
            g.Sports.Select(s => s.Sport.Name ?? string.Empty).ToList(),
            (int)g.HourlyRate,
            "PKR",
            g.CreatedAt)).ToList();

        return Result.Success<IReadOnlyList<FacilityResponse>>(result);
    }
}
