You are working on a .NET 10 Clean Architecture (Hexagonal / Ports & Adapters) project called Slot.
The solution structure is:

  Slot.Application         — domain models, ports (IAccountService, IUserRepository), services
  Slot.Adapters.PostgreSql — EF Core + Identity repositories, ApplicationDbContext
  Slot.Adapters.FastEndpoint — FastEndpoints 7.x HTTP layer
  Slot.Host                — ASP.NET Core host, Program.cs

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

EXISTING CODE (do not change these, only extend):

// Slot.Application/Models/User.cs
public class User : FullyAuditedEntity<Guid>
{
    public string? ImageUrl { get; set; }
    public string UserIdentityId { get; set; } = null!;
    public IdentityUser UserIdentity { get; set; } = null!;
    public ICollection<Ground> OwnedGrounds { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}

// Slot.Application/Ports/In/Account/IAccountService.cs
public interface IAccountService
{
    Task<Result<LoginResponse>>  LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result>                 SignUpAsync(SignUpRequest request, CancellationToken ct = default);
    Task<Result>                 ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task<Result>                 ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<Result<UserProfileResponse>> GetProfileAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result>                 UpdateProfileAsync(string userIdentityId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<Result>                 ChangePasswordAsync(string userIdentityId, ChangePasswordRequest request, CancellationToken ct = default);
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string Email, Guid UserId);
public record SignUpRequest(string Email, string Password, string FullName, string? PhoneNumber);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);

// --- ADD THESE NEW RECORDS to the same file ---
public record UserProfileResponse(Guid UserId, string Email, string? FullName, string? PhoneNumber, string? ImageUrl, IList<string> Roles);
public record UpdateProfileRequest(string? FullName, string? PhoneNumber, string? ImageUrl);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

// Slot.Application/Ports/Out/Persistence/RepositoryContracts/IUserRepository.cs
public interface IUserRepository
{
    Task<(IdentityUser? identity, User? profile)> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<(IdentityUser? identity, User? profile)> FindByIdentityIdAsync(string identityId, CancellationToken ct = default);
    Task CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}

// Slot.Common/Results/Result.cs  (already exists — shown for context)
public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}
public class Result<T> : Result { public T? Value { get; } }

// Slot.Common/Results/Error.cs  (already exists — shown for context)
public class Error
{
    public string Message { get; }
    public int Code { get; }
    public static Error NotFound(string msg)     => new(msg, 404);
    public static Error UnAuthorized(string msg) => new(msg, 401);
    public static Error Validation(string msg)   => new(msg, 400);
    public static Error Conflict(string msg)     => new(msg, 409);
}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

TASK — implement the following 5 endpoints in order:

─────────────────────────────────────────────
1. POST /api/account/forgot-password
─────────────────────────────────────────────
Request:  { "email": "user@slot.com" }
Response: { "success": true, "message": "If an account exists, a reset token has been sent." }
Auth:     Anonymous

Flow:
  - Call accountService.ForgotPasswordAsync
  - In AccountService: call userManager.FindByEmailAsync
  - If user not found, still return Result.Success() — never reveal if email exists
  - If found: call userManager.GeneratePasswordResetTokenAsync
  - Log the token to Console in dev (email service is not built yet)
  - Always return 200 OK regardless

─────────────────────────────────────────────
2. POST /api/account/reset-password
─────────────────────────────────────────────
Request:  { "email": "user@slot.com", "token": "CfD...", "newPassword": "NewPass@123" }
Response: { "success": true } or { "success": false, "error": "Invalid token." }
Auth:     Anonymous

Flow:
  - Call accountService.ResetPasswordAsync
  - In AccountService: FindByEmailAsync — if null return Error.NotFound
  - Call userManager.ResetPasswordAsync(identityUser, token, newPassword)
  - If result.Succeeded is false, join all IdentityError.Description into one string, return Error.Validation
  - Return Result.Success()

─────────────────────────────────────────────
3. GET /api/account/me
─────────────────────────────────────────────
Request:  — (JWT Bearer token in Authorization header)
Response:
  {
    "userId": "guid",
    "email": "user@slot.com",
    "fullName": null,
    "phoneNumber": "0300-0000000",
    "imageUrl": null,
    "roles": ["User"]
  }
Auth:     JWT required — extract sub claim (IdentityUser.Id) from HttpContext.User

Flow:
  - Endpoint reads identityId from HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
    (this is the IdentityUser.Id string stored in the "sub" claim at login)
  - Call accountService.GetProfileAsync(identityId)
  - In AccountService: call userRepository.FindByIdentityIdAsync(identityId)
  - If null, return Error.NotFound("User not found.")
  - Call userManager.GetRolesAsync(identityUser) to get roles
  - Map to UserProfileResponse — note: FullName and PhoneNumber are on IdentityUser
    (IdentityUser.UserName holds email; PhoneNumber is IdentityUser.PhoneNumber;
     FullName is NOT on IdentityUser — check if User model has a FullName property;
     if not, return null for fullName for now)
  - Return Result.Success(profileResponse)

─────────────────────────────────────────────
4. PUT /api/account/me
─────────────────────────────────────────────
Request:  { "fullName": "Ahmed Khan", "phoneNumber": "0311-1234567", "imageUrl": "https://..." }
Response: { "success": true } or { "success": false, "error": "..." }
Auth:     JWT required

Flow:
  - Endpoint reads identityId from ClaimTypes.NameIdentifier
  - Call accountService.UpdateProfileAsync(identityId, request)
  - In AccountService: FindByIdentityIdAsync — if null return Error.NotFound
  - Update IdentityUser.PhoneNumber = request.PhoneNumber
  - Call userManager.UpdateAsync(identityUser) — check result.Succeeded
  - Update User.ImageUrl = request.ImageUrl (if provided)
  - Call userRepository.UpdateAsync(userProfile)
  - Return Result.Success()

─────────────────────────────────────────────
5. PUT /api/account/change-password
─────────────────────────────────────────────
Request:  { "currentPassword": "Hello@123", "newPassword": "NewPass@456" }
Response: { "success": true } or { "success": false, "error": "Current password is incorrect." }
Auth:     JWT required

Flow:
  - Endpoint reads identityId from ClaimTypes.NameIdentifier
  - Call accountService.ChangePasswordAsync(identityId, request)
  - In AccountService: FindByIdentityIdAsync — if null return Error.NotFound
  - Call userManager.ChangePasswordAsync(identityUser, currentPassword, newPassword)
  - If not Succeeded: join errors, return Error.Validation
  - Return Result.Success()

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

FILES TO CREATE OR UPDATE (in this exact order):

1. UPDATE  Slot.Application/Ports/In/Account/IAccountService.cs
           — add 3 new method signatures + 3 new records listed above

2. UPDATE  Slot.Application/Ports/Out/Persistence/RepositoryContracts/IUserRepository.cs
           — add FindByIdentityIdAsync and UpdateAsync signatures

3. UPDATE  Slot.Adapters.PostgreSql/Repositories/UserRepository.cs
           — implement FindByIdentityIdAsync: query db.Users by UserIdentityId == identityId,
             also fetch the IdentityUser via userManager.FindByIdAsync(identityId)
           — implement UpdateAsync: db.Users.Update(user); await db.SaveChangesAsync(ct)

4. UPDATE  Slot.Application/Services/AccountService.cs
           — implement GetProfileAsync, UpdateProfileAsync, ChangePasswordAsync
           — keep existing LoginAsync, SignUpAsync, ForgotPasswordAsync, ResetPasswordAsync intact

5. NEW     Slot.Adapters.FastEndpoint/Endpoints/Account/ForgotPassword/ForgotPasswordEndpointRequest.cs
6. NEW     Slot.Adapters.FastEndpoint/Endpoints/Account/ForgotPassword/ForgotPasswordEndpointResponse.cs
7. NEW     Slot.Adapters.FastEndpoint/Endpoints/Account/ForgotPassword/ForgotPasswordEndpoint.cs

8. NEW     Slot.Adapters.FastEndpoint/Endpoints/Account/ResetPassword/ResetPasswordEndpointRequest.cs
9. NEW     Slot.Adapters.FastEndpoint/Endpoints/Account/ResetPassword/ResetPasswordEndpointResponse.cs
10. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/ResetPassword/ResetPasswordEndpoint.cs

11. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/GetProfile/GetProfileEndpointResponse.cs
12. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/GetProfile/GetProfileEndpoint.cs
            (no request file needed — no body, just JWT)

13. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/UpdateProfile/UpdateProfileEndpointRequest.cs
14. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/UpdateProfile/UpdateProfileEndpointResponse.cs
15. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/UpdateProfile/UpdateProfileEndpoint.cs

16. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/ChangePassword/ChangePasswordEndpointRequest.cs
17. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/ChangePassword/ChangePasswordEndpointResponse.cs
18. NEW    Slot.Adapters.FastEndpoint/Endpoints/Account/ChangePassword/ChangePasswordEndpoint.cs

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

CONVENTIONS TO FOLLOW (match existing code exactly):

Endpoints:
  - Inherit Endpoint<TReq, TRes> or EndpointWithoutRequest<TRes> for GET /me
  - Configure(): Post/Put/Get route, AllowAnonymous() or RequireAuthorization(),
    Summary(s => { s.Summary = "..."; s.Response<TRes>(200, "..."); s.Response(400, "..."); })
  - No Tags() call — AutoTagPathSegments = 1 handles tagging
  - Use Send.OkAsync(response, ct) for 200
  - Use Send.ResponseAsync(response, result.Error.Code, ct) for errors
  - All request classes use public properties with { get; set; } = null!  (not records)
  - All response classes use public properties with { get; set; }         (not records)
  - Extract identity id in endpoint: HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
  - using Microsoft.AspNetCore.Identity; using System.Security.Claims; at top of endpoint files

AccountService:
  - Constructor injection: IUserRepository userRepository, UserManager<IdentityUser> userManager,
    IOptions<JwtSettings> jwtOptions
  - All methods async Task<Result> or Task<Result<T>>
  - Never throw exceptions — always return Result.Failure(Error.Xxx(...))

UserRepository:
  - Constructor injection: ApplicationDbContext db, UserManager<IdentityUser> userManager
  - Use userManager for all IdentityUser queries (FindByEmailAsync, FindByIdAsync, UpdateAsync)
  - Use db.Users for User profile queries
  - AsNoTracking() on reads, no AsNoTracking on writes

GlobalUsings already present in each project — no need to add using FastEndpoints or
using Slot.Application.Ports.In.Account in endpoint files; they are global.