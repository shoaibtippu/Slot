using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Slot.Application.Configurations;
using Slot.Application.Ports.In.Account;
using Slot.Application.Ports.Out.Persistence.RepositoryContracts;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Slot.Application.Services;

public class AccountService(
    IUserRepository userRepository,
    UserManager<IdentityUser> userManager,
    IOptions<JwtSettings> jwtOptions) : IAccountService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    #region Login

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var (identityUser, userProfile) = await userRepository.FindByEmailAsync(request.Email, ct);

        if (identityUser is null || userProfile is null)
            return Result.Failure<LoginResponse>(Error.UnAuthorized("Invalid email or password."));

        var passwordValid = await userManager.CheckPasswordAsync(identityUser, request.Password);
        if (!passwordValid)
            return Result.Failure<LoginResponse>(Error.UnAuthorized("Invalid email or password."));

        var roles = await userManager.GetRolesAsync(identityUser);
        var token = GenerateJwt(identityUser, userProfile.Id, roles);

        return Result.Success(new LoginResponse(token, identityUser.Email!, userProfile.Id));
    }

    private string GenerateJwt(IdentityUser identity, Guid userId, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, identity.Id),
            new(JwtRegisteredClaimNames.Email, identity.Email!),
            new("userId", userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion

    #region SignUp

    public async Task<Result> SignUpAsync(SignUpRequest request, CancellationToken ct = default)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Result.Failure(Error.Conflict("An account with this email already exists."));

        if (request.Role is not (SystemRole.User or SystemRole.GroundOwner))
            return Result.Failure(Error.Validation("Invalid role selected."));

        var identityUser = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var createResult = await userManager.CreateAsync(identityUser, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation(errors));
        }

        await userManager.AddToRoleAsync(identityUser, "User");

        var userProfile = new User
        {
            Id = Guid.NewGuid(),
            UserIdentityId = identityUser.Id,
            FullName = request.FullName,
            City = request.City,
        };

        await userRepository.CreateAsync(userProfile, ct);

        return Result.Success();
    }

    #endregion

    #region ForgotPassword

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var identityUser = await userManager.FindByEmailAsync(request.Email);

        // Always return success to avoid email enumeration attacks
        if (identityUser is null)
            return Result.Success();

        var token = await userManager.GeneratePasswordResetTokenAsync(identityUser);

        // TODO: plug in your email service here and send the token
        // e.g. await emailService.SendPasswordResetAsync(request.Email, token);
        // For now we expose the token in the result so you can test via Swagger
        Console.WriteLine($"[DEV] Password reset token for {request.Email}: {token}");

        return Result.Success();
    }

    #endregion

    #region ResetPassword

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var identityUser = await userManager.FindByEmailAsync(request.Email);
        if (identityUser is null)
            return Result.Failure(Error.NotFound("No account found with this email."));

        var resetResult = await userManager.ResetPasswordAsync(identityUser, request.Token, request.NewPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation(errors));
        }

        return Result.Success();
    }

    #endregion

    #region GetProfile

    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identityUser, userProfile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);

        if (identityUser is null || userProfile is null)
            return Result.Failure<UserProfileResponse>(Error.NotFound("User not found."));

        var roles = await userManager.GetRolesAsync(identityUser);

        return Result.Success(new UserProfileResponse(
            userProfile.Id,
            identityUser.Email!,
            null,
            identityUser.PhoneNumber,
            userProfile.ImageUrl,
            roles.ToList()));
    }

    #endregion

    #region UpdateProfile

    public async Task<Result> UpdateProfileAsync(string userIdentityId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var (identityUser, userProfile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);

        if (identityUser is null || userProfile is null)
            return Result.Failure(Error.NotFound("User not found."));

        identityUser.PhoneNumber = request.PhoneNumber;

        var updateIdentityResult = await userManager.UpdateAsync(identityUser);
        if (!updateIdentityResult.Succeeded)
        {
            var errors = string.Join(", ", updateIdentityResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation(errors));
        }

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            userProfile.ImageUrl = request.ImageUrl;

        await userRepository.UpdateAsync(userProfile, ct);

        return Result.Success();
    }

    #endregion

    #region ChangePassword

    public async Task<Result> ChangePasswordAsync(string userIdentityId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var (identityUser, userProfile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);

        if (identityUser is null || userProfile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var changePasswordResult = await userManager.ChangePasswordAsync(identityUser, request.CurrentPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation(errors));
        }

        return Result.Success();
    }

    #endregion
}