namespace WordoGuessr.Auth.App.Abstractions;

public sealed class UserServiceException : Exception
{
    public UserServiceException(params UserServiceError[] errors)
        : base(CreateMessage(errors))
    {
        Errors = errors;
    }

    public IReadOnlyCollection<UserServiceError> Errors { get; }

    private static string CreateMessage(UserServiceError[] errors)
        => errors.Length == 0
            ? "An unknown user service operation error occurred."
            : $"User service operation failed: {string.Join("; ", errors.Select(error => $"{error.Code}: {error.Description}"))}";
}

public sealed record UserServiceError(string Code, string Description);
