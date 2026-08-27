using Slot.Application.Ports.Out.Persistence.RepositoryContracts;

namespace Slot.Application.Ports.In.Facilities;

public interface IFacilityService
{
    Task<Result<IReadOnlyList<FacilityResponse>>> GetAllAsync(CancellationToken ct = default);
}