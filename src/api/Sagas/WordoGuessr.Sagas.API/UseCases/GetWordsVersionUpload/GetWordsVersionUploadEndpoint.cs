using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.API.Security;
using WordoGuessr.Sagas.App.UseCases.GetWordsVersionUpload;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.API.UseCases.GetWordsVersionUpload;

internal static class GetWordsVersionUploadEndpoint
{
    public static IEndpointRouteBuilder MapGetWordsVersionUpload(this IEndpointRouteBuilder group)
    {
        group.MapGet("/words-version-uploads/{uploadId:guid}", Handle)
            .RequireAuthorization(Permissions.ViewWordsVersionUpload)
            .WithName("GetWordsVersionUpload");

        return group;
    }

    private static async Task<Ok<GetWordsVersionUploadResponse>> Handle(
        [FromRoute] Guid uploadId,
        [FromServices] IQueryHandler<GetWordsVersionUploadQuery, UploadWordsVersionSagaDto?> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var currentUpload = await handler.Handle(new GetWordsVersionUploadQuery(uploadId), ct);
        return TypedResults.Ok(new GetWordsVersionUploadResponse(currentUpload));
    }
}
