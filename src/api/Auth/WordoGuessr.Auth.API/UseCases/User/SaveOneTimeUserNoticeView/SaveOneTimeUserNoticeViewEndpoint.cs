using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.UseCases.User.SaveOneTimeUserNoticeView;

namespace WordoGuessr.Auth.API.UseCases.User.SaveOneTimeUserNoticeView;

internal static class SaveOneTimeUserNoticeViewEndpoint
{
    public static IEndpointRouteBuilder MapSaveOneTimeUserNoticeView(this IEndpointRouteBuilder group)
    {
        group.MapPost("/user-notice/views/one-time", Handle)
            .ProducesValidationProblem()
            .WithName("SaveOneTimeUserNoticeView");

        return group;
    }

    private static async Task<Ok> Handle(
        SaveOneTimeUserNoticeViewRequest request,
        ICurrentPlayerAccessor currentPlayerAccessor,
        ICommandHandler<SaveOneTimeUserNoticeViewCommand> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        await handler.Handle(new SaveOneTimeUserNoticeViewCommand(currentPlayer.PlayerId, request.NoticeCode), ct);
        return TypedResults.Ok();
    }
}