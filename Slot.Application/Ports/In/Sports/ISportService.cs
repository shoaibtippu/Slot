namespace Slot.Application.Ports.In.Sports;

public interface ISportService
{
    Task<Result<IReadOnlyList<SportResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<SportResponse>> CreateAsync(CreateSportRequest request, CancellationToken ct = default);
    Task<Result<SportResponse>> UpdateAsync(Guid id, UpdateSportRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public record SportResponse(Guid Id, string Name, string? IconUrl);
public record CreateSportRequest(string Name, string? IconUrl);
public record UpdateSportRequest(string Name, string? IconUrl);