using Slot.Common.Results;

namespace Slot.Common.Models;

public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(bool isSuccess, T? value, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}