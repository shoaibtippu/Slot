namespace Slot.Application.Ports.In.Account.SignUp;

public record SignUpRequest(string Email, string Password, string FullName, string? PhoneNumber, string? City,
    SystemRole Role);