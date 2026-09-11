namespace Slot.Adapters.FastEndpoint.Endpoints.Users.SearchUsers;

public class SearchUsersEndpointResponse
{
    public IReadOnlyList<UserSearchResult> Users { get; set; } = [];
    public string? Error { get; set; }
}

public class UserSearchResult
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
}
