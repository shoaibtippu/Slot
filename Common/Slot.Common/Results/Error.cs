namespace Slot.Common.Results;

public record Error(int Code, string Message)
{
    public static readonly Error None = new(0, string.Empty);

    public static Error NotFound(string message = "Resource not found.") => new(404, message);
    public static Error Validation(string message = "Validation failed.") => new(400, message);
    public static Error Conflict(string message = "Conflict occurred.") => new(409, message);
    public static Error Unexpected(string message = "An unexpected error occurred.") => new(500, message);
    public static Error UnAuthorized(string message = "Unauthorized.") => new(401, message);
}