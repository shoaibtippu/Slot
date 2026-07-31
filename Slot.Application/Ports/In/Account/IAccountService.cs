namespace Slot.Application.Ports.In.Account;

public interface IAccountService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result> SignUpAsync(SignUpRequest request, CancellationToken ct = default);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<Result<UserProfileResponse>> GetProfileAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result> UpdateProfileAsync(string userIdentityId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<Result> ChangePasswordAsync(string userIdentityId, ChangePasswordRequest request, CancellationToken ct = default);
}

public record UserProfileResponse(Guid UserId, string Email, string? FullName, string? PhoneNumber, string? ImageUrl, IList<string> Roles);
public record UpdateProfileRequest(string? FullName, string? PhoneNumber, string? ImageUrl);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);