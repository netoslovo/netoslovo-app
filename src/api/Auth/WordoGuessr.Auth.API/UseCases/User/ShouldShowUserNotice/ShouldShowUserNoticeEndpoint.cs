using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.UseCases.User.ShouldShowUserNotice;

namespace WordoGuessr.Auth.API.UseCases.User.ShouldShowUserNotice;

internal static class ShouldShowUserNoticeEndpoint
{
    public static IEndpointRouteBuilder MapShouldShowUserNotice(this IEndpointRouteBuilder group)
    {
        group.MapGet("/user-notices/should-show", Handle)
            .WithName("ShouldShowUserNotice");

        return group;
    }

    private static async Task<Ok<bool>> Handle(
        string noticeCode,
        IQueryHandler<ShouldShowUserNoticeQuery, bool> handler,
        ICurrentPlayerAccessor currentPlayerAccessor,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var shouldShow = await handler.Handle(
            new ShouldShowUserNoticeQuery(currentPlayer.PlayerId, noticeCode),
            ct);

        return TypedResults.Ok(shouldShow);
    }
}
