namespace Slot.Adapters.FastEndpoint.Endpoints.Account.ForgotPassword;

public class ForgotPassword(IAccountService accountService) : Endpoint<ForgotPasswordEndpointRequest, ForgotPasswordEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/account/forgot-password");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Request a password reset token for an account.";
            s.Response<ForgotPasswordEndpointResponse>(200, "Password reset request accepted.");
        });
    }

    public override async Task HandleAsync(ForgotPasswordEndpointRequest req, CancellationToken ct)
    {
        await accountService.ForgotPasswordAsync(new ForgotPasswordRequest(req.Email), ct);

        await Send.OkAsync(new ForgotPasswordEndpointResponse
        {
            Success = true,
            Message = "If an account exists, a reset token has been sent."
        }, ct);
    }
}