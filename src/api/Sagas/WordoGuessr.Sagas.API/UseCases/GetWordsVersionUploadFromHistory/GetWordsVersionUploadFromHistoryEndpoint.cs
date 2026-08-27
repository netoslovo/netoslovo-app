using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.API.Security;
using WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadFromHistory;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.API.UseCases.GetWordsVersionUploadFromHistory;

internal static class GetWordsVersionUploadFromHistoryEndpoint
{
    public static IEndpointRouteBuilder MapGetWordsVersionUploadFromHistory(this IEndpointRouteBuilder group)
    {
        group.MapGet("/words-version-uploads/history/{uploadId:guid}", Handle)
            .RequireAuthorization(Permissions.ViewWordsVersionUpload)
            .WithName("GetWordsVersionUploadFromHistory");

        return group;
    }

    private static async Task<Ok<GetWordsVersionUploadFromHistoryResponse>> Handle(
        [FromRoute] Guid uploadId,
        [FromServices] IQueryHandler<GetWordsVersionUploadFromHistoryQuery, UploadWordsVersionSagaHistoryDto?> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var upload = await handler.Handle(new GetWordsVersionUploadFromHistoryQuery(uploadId), ct);
        return TypedResults.Ok(new GetWordsVersionUploadFromHistoryResponse(upload));
    }
}
