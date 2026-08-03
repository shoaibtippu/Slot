using Slot.Application.Ports.In.Account.SignUp;

namespace Slot.Adapters.FastEndpoint.Endpoints.Account.SignUp;

public class SignUp(IAccountService accountService) : Endpoint<SignUpEndpointRequest, SignUpEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/account/signup");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Register a new user account.";
            s.Response<SignUpEndpointResponse>(200, "Registration successful");
            s.Response(400, "Validation error or email already exists");
        });
    }

    public override async Task HandleAsync(SignUpEndpointRequest req, CancellationToken ct)
    {
        var result = await accountService.SignUpAsync(
            new SignUpRequest(req.Email, req.Password, req.FullName, req.PhoneNumber, req.City,
                req.Role), ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(
                new SignUpEndpointResponse { Success = false, Error = result.Error!.Message },
                result.Error.Code,
                ct);
            return;
        }

        await Send.OkAsync(
            new SignUpEndpointResponse { Success = true, Error = null },
            ct);
    }
}