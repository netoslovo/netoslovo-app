using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Common.Domain;
using WordoGuessr.Sagas.API.Security;
using WordoGuessr.Sagas.App.UseCases.CancelWordsVersionUpload;

namespace WordoGuessr.Sagas.API.UseCases.CancelWordsVersionUpload;

internal static class CancelWordsVersionUploadEndpoint
{
    public static IEndpointRouteBuilder MapCancelWordsVersionUpload(this IEndpointRouteBuilder group)
    {
        group.MapPost("/words-version-uploads/{uploadId:guid}/cancel", Handle)
            .RequireAuthorization(Permissions.CancelWordsVersionUpload)
            .WithName("CancelWordsVersionUpload");

        return group;
    }

    private static async Task<Results<Ok, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>> Handle(
        [FromRoute] Guid uploadId,
        [FromServices] ICommandHandler<CancelWordsVersionUploadCommand, Result<CancelWordsVersionUploadError>> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var command = new CancelWordsVersionUploadCommand(uploadId);
        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok();
    }

    private static Results<Ok, NotFound<ProblemDetails>, BadRequest<ProblemDetails>> ToResult(
        CancelWordsVersionUploadError error,
        CancelWordsVersionUploadCommand command) =>
        error switch
        {
            CancelWordsVersionUploadError.NotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Words version upload was not found",
                    $"Words version upload '{command.SagaId}' was not found.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Cancel words version upload request failed",
                    "The cancel words version upload request could not be processed.",
                    error.ToString()))
        };
}
