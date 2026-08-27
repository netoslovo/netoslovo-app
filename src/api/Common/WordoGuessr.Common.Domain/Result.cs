using System.Diagnostics.CodeAnalysis;

namespace WordoGuessr.Common.Domain;

public sealed class Result<T, TError>
{
    private Result(T? value, TError? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public T? Value { get; }
    public TError? Error { get; }

    public static Result<T, TError> Success(T value) =>
        new Result<T, TError>(value, default, true);

    public static Result<T, TError> Failure(TError error) =>
        new Result<T, TError>(default, error, false);
}

public sealed class Result<TError>
{
    private Result(TError? error, bool isSuccess)
    {
        Error = error;
        IsSuccess = isSuccess;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public TError? Error { get; }

    public static Result<TError> Success() =>
        new Result<TError>(default, true);

    public static Result<TError> Failure(TError error) =>
        new Result<TError>(error, false);
}
