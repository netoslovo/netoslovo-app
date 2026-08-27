using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.UseCases.User.SaveRecurringUserNoticeView;

namespace WordoGuessr.Auth.API.UseCases.User.SaveRecurringUserNoticeView;

internal static class SaveRecurringUserNoticeViewEndpoint
{
    public static IEndpointRouteBuilder MapSaveRecurringUserNoticeView(this IEndpointRouteBuilder group)
    {
        group.MapPost("/user-notices/views/recurring", Handle)
            .ProducesValidationProblem()
            .WithName("SaveRecurringUserNoticeView");

        return group;
    }

    private static async Task<Ok> Handle(
        SaveRecurringUserNoticeViewRequest request,
        ICurrentPlayerAccessor currentPlayerAccessor,
        ICommandHandler<SaveRecurringUserNoticeViewCommand> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        await handler.Handle(
            new SaveRecurringUserNoticeViewCommand(
                currentPlayer.PlayerId,
                request.NoticeCode,
                request.DoNotShowAgain),
            ct);
        return TypedResults.Ok();
    }
}