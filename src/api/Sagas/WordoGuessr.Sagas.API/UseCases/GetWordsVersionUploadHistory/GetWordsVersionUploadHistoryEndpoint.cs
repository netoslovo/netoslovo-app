using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.API.Security;
using WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadHistory;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.API.UseCases.GetWordsVersionUploadHistory;

internal static class GetWordsVersionUploadHistoryEndpoint
{
    public static IEndpointRouteBuilder MapGetWordsVersionUploadHistory(this IEndpointRouteBuilder group)
    {
        group.MapGet("/words-version-uploads/history", Handle)
            .RequireAuthorization(Permissions.ViewWordsVersionUpload)
            .ProducesValidationProblem()
            .WithName("GetWordsVersionUploadHistory");

        return group;
    }

    private static async Task<Ok<UploadWordsVersionSagaHistoryPageDto>> Handle(
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int? wordsVersion,
        [FromQuery] UploadWordsVersionSagaHistorySortFieldDto? sortBy,
        [FromQuery] SortDirectionDto? sortDirection,
        [FromServices] IQueryHandler<GetWordsVersionUploadHistoryQuery, UploadWordsVersionSagaHistoryPageDto> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var query = new GetWordsVersionUploadHistoryQuery(
            skip,
            take,
            from,
            to,
            wordsVersion,
            sortBy,
            sortDirection);
        var page = await handler.Handle(query, ct);

        return TypedResults.Ok(page);
    }
}
