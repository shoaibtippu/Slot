using System.Text.Json.Serialization;

namespace Slot.Common.Dto;

public record PagedSearchSortDto
{
    [JsonPropertyName("page_number")]
    public int PageNumber { get; init; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; init; }

    [JsonPropertyName("order_by")]
    public string? OrderBy { get; init; }

    [JsonPropertyName("search")]
    public string? Search { get; init; }
}