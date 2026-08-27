using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.API.Security;
using WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploads;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.API.UseCases.GetWordsVersionUploads;

internal static class GetWordsVersionUploadsEndpoint
{
    public static IEndpointRouteBuilder MapGetWordsVersionUploads(this IEndpointRouteBuilder group)
    {
        group.MapGet("/words-version-uploads", Handle)
            .RequireAuthorization(Permissions.ViewWordsVersionUpload)
            .ProducesValidationProblem()
            .WithName("GetWordsVersionUploads");

        return group;
    }

    private static async Task<Ok<UploadWordsVersionSagaPageDto>> Handle(
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromServices] IQueryHandler<GetWordsVersionUploadsQuery, UploadWordsVersionSagaPageDto> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var page = await handler.Handle(new GetWordsVersionUploadsQuery(skip, take), ct);

        return TypedResults.Ok(page);
    }
}
