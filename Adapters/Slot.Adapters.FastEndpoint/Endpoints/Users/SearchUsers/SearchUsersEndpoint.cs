using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Users.SearchUsers;

[Authorize]
public class SearchUsers(IUserRepository userRepository) : EndpointWithoutRequest<SearchUsersEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/users/search");
        Summary(s =>
        {
            s.Summary = "Search users by email.";
            s.Response<SearchUsersEndpointResponse>(200, "Users found.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new SearchUsersEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        var name = Query<string>("name", isRequired: false) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        {
            await Send.OkAsync(new SearchUsersEndpointResponse { Users = [], Error = null }, ct);
            return;
        }

        var results = await userRepository.SearchByNameAsync(name, 20, ct);
        var users = results
            .Select(r => new UserSearchResult
            {
                Id = r.profile.Id,
                FullName = r.profile.FullName,
                Email = r.identity.Email,
                ImageUrl = r.profile.ImageUrl
            })
            .ToList();

        await Send.OkAsync(new SearchUsersEndpointResponse { Users = users, Error = null }, ct);
    }
}
