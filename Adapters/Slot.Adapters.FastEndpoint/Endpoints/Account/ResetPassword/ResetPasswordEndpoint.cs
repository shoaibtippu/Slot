namespace Slot.Adapters.FastEndpoint.Endpoints.Account.ResetPassword;

public class ResetPassword(IAccountService accountService) : Endpoint<ResetPasswordEndpointRequest, ResetPasswordEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/account/reset-password");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Reset a password using a valid token.";
            s.Response<ResetPasswordEndpointResponse>(200, "Password reset successful.");
            s.Response(400, "Invalid token or request.");
        });
    }

    public override async Task HandleAsync(ResetPasswordEndpointRequest req, CancellationToken ct)
    {
        var result = await accountService.ResetPasswordAsync(
            new ResetPasswordRequest(req.Email, req.Token, req.NewPassword), ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new ResetPasswordEndpointResponse
            {
                Success = false,
                Error = result.Error!.Message
            }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new ResetPasswordEndpointResponse
        {
            Success = true,
            Error = null
        }, ct);
    }
}