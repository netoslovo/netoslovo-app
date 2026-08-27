using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;

namespace WordoGuessr.WebAPI.CurrentUser;

internal sealed class ActorStampMiddleware
{
    public const string HeaderName = "X-Actor-Stamp";

    private readonly RequestDelegate _next;

    public ActorStampMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentPlayerAccessor currentPlayerAccessor)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var headerValue = currentPlayer.IsAuthenticated
            ? $"authenticated:{currentPlayer.AuthenticatedUserId}"
            : "guest";

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = headerValue;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

internal static class ActorStampMiddlewareExtensions
{
    public static IApplicationBuilder UseActorStamp(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<ActorStampMiddleware>();
    }
}