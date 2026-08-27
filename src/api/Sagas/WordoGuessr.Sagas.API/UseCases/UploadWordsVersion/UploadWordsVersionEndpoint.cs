using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Common.Domain;
using WordoGuessr.Sagas.API.Security;
using WordoGuessr.Sagas.App.UseCases.UploadWordsVersion;

namespace WordoGuessr.Sagas.API.UseCases.UploadWordsVersion;

internal static class UploadWordsVersionEndpoint
{
    public static IEndpointRouteBuilder MapUploadWordsVersion(this IEndpointRouteBuilder group)
    {
        group.MapPost("/words-version-uploads", Handle)
            .RequireAuthorization(Permissions.UploadWordsVersion)
            .WithName("CreateWordsVersionUpload");

        return group;
    }

    private static async Task<Results<Ok<Guid>, Conflict<ProblemDetails>, BadRequest<ProblemDetails>>> Handle(
        [FromBody] UploadWordsVersionRequest request,
        [FromServices] ICommandHandler<UploadWordsVersionCommand, Result<UploadWordsVersionError>> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var uploadId = Guid.CreateVersion7();
        var command = new UploadWordsVersionCommand(uploadId, request.Version);
        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error);
        }

        return TypedResults.Ok(uploadId);
    }

    private static Results<Ok<Guid>, Conflict<ProblemDetails>, BadRequest<ProblemDetails>> ToResult(UploadWordsVersionError error) =>
        error switch
        {
            UploadWordsVersionError.AlreadyRunning => TypedResults.Conflict(ProblemDetailsMapping.Create(
                StatusCodes.Status409Conflict,
                "Words version upload is already running",
                "A words version upload is already running. Wait until it finishes before starting another upload.",
                error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Words version upload failed",
                    "Words version upload request could not be processed.",
                    error.ToString()))
        };
}
