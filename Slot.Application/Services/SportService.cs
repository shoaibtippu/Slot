namespace Slot.Application.Services;

public class SportService(ISportRepository sportRepository) : ISportService
{
    public async Task<Result<IReadOnlyList<SportResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var sports = await sportRepository.GetAllAsync(ct);
        var response = sports.Select(s => new SportResponse(s.Id, s.Name, s.IconUrl)).ToList();
        return Result.Success<IReadOnlyList<SportResponse>>(response);
    }

    public async Task<Result<SportResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sport = await sportRepository.FindByIdAsync(id, ct);
        if (sport is null)
            return Result.Failure<SportResponse>(Error.NotFound("Sport not found."));

        return Result.Success(new SportResponse(sport.Id, sport.Name, sport.IconUrl));
    }

    public async Task<Result<SportResponse>> CreateAsync(CreateSportRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<SportResponse>(Error.Validation("Name is required."));

        var name = request.Name.Trim();
        if (await sportRepository.ExistsByNameAsync(name, null, ct))
            return Result.Failure<SportResponse>(Error.Conflict("A sport with this name already exists."));

        var sport = new Sport
        {
            Id = Guid.NewGuid(),
            Name = name,
            IconUrl = string.IsNullOrWhiteSpace(request.IconUrl) ? null : request.IconUrl.Trim()
        };

        await sportRepository.CreateAsync(sport, ct);
        return Result.Success(new SportResponse(sport.Id, sport.Name, sport.IconUrl));
    }

    public async Task<Result<SportResponse>> UpdateAsync(Guid id, UpdateSportRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<SportResponse>(Error.Validation("Name is required."));

        var sport = await sportRepository.FindByIdAsync(id, ct);
        if (sport is null)
            return Result.Failure<SportResponse>(Error.NotFound("Sport not found."));

        var name = request.Name.Trim();
        if (await sportRepository.ExistsByNameAsync(name, id, ct))
            return Result.Failure<SportResponse>(Error.Conflict("A sport with this name already exists."));

        sport.Name = name;
        sport.IconUrl = string.IsNullOrWhiteSpace(request.IconUrl) ? null : request.IconUrl.Trim();

        await sportRepository.UpdateAsync(sport, ct);
        return Result.Success(new SportResponse(sport.Id, sport.Name, sport.IconUrl));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sport = await sportRepository.FindByIdAsync(id, ct);
        if (sport is null)
            return Result.Failure(Error.NotFound("Sport not found."));

        await sportRepository.DeleteAsync(sport, ct);
        return Result.Success();
    }
}