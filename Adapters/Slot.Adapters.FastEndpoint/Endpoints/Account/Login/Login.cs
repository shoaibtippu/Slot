namespace Slot.Adapters.FastEndpoint.Endpoints.Account.Login;

public class Login(IAccountService accountService) : Endpoint<LoginEndpointRequest, LoginEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/v1/account/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Authenticate a user and return a JWT token.";
            s.Response<LoginEndpointResponse>(200, "Login successful");
            s.Response(400, "Invalid credentials");
        });
    }

    public override async Task HandleAsync(LoginEndpointRequest req, CancellationToken ct)
    {
        var result = await accountService.LoginAsync(new LoginRequest(req.Email, req.Password), ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(
                new LoginEndpointResponse
                (
                    AccessToken : null,
                    Email : null,
                    UserId : null,
                    Error : result.Error!.Message
                ),
                result.Error.Code,
                ct);
            return;
        }

        await Send.OkAsync(
            new LoginEndpointResponse
            (
                AccessToken : result.Value!.AccessToken,
                Email : result.Value.Email,
                UserId : result.Value.UserId,
                Error : null
            ),
            ct);
    }
}