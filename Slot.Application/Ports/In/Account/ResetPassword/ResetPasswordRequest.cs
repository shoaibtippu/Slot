namespace Slot.Application.Ports.In.Account.ResetPassword;

public record ResetPasswordRequest(string Email, string Token, string NewPassword);