namespace WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;

public sealed class CurrentPlayerException : Exception
{
    private const string CurrentUserUnauthorizedExceptionMessage
        = "Current player is not available for this request.";

    public CurrentPlayerException()
        : base(CurrentUserUnauthorizedExceptionMessage)
    {
    }

    public CurrentPlayerException(Exception ex)
        : base(CurrentUserUnauthorizedExceptionMessage, ex)
    {
    }

    public CurrentPlayerException(string message)
        : base($"{CurrentUserUnauthorizedExceptionMessage}. Details: {message}")
    {
    }
}
