using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;
using WordoGuessr.Sagas.API.Security;
using WordoGuessr.Sagas.API.UseCases.CancelWordsVersionUpload;
using WordoGuessr.Sagas.API.UseCases.GetWordsVersionUpload;
using WordoGuessr.Sagas.API.UseCases.GetWordsVersionUploadFromHistory;
using WordoGuessr.Sagas.API.UseCases.GetWordsVersionUploadHistory;
using WordoGuessr.Sagas.API.UseCases.GetWordsVersionUploads;
using WordoGuessr.Sagas.API.UseCases.UploadWordsVersion;

namespace WordoGuessr.Sagas.API;

public static class SagasApiExtensions
{
    public static IServiceCollection AddSagasApi(this IServiceCollection services)
    {
        services.AddTransient<IModuleClaimsTransformation, SagasPermissionClaimsTransformation>();
        services.AddAuthorization(options => options.AddSagasPolicies());

        return services;
    }

    public static IEndpointRouteBuilder MapSagasEndpoints(this IEndpointRouteBuilder app)
    {
        var sagasAdminGroup = app
            .MapGroup("/api/sagas/admin")
            .WithTags("Sagas")
            .RequireAuthorization()
            .WithMetadata(
                new ProducesResponseTypeAttribute(
                    typeof(ProblemDetails),
                    StatusCodes.Status401Unauthorized),
                new ProducesResponseTypeAttribute(
                    typeof(ProblemDetails),
                    StatusCodes.Status403Forbidden));

        sagasAdminGroup
            .MapUploadWordsVersion()
            .MapCancelWordsVersionUpload()
            .MapGetWordsVersionUploads()
            .MapGetWordsVersionUpload()
            .MapGetWordsVersionUploadFromHistory()
            .MapGetWordsVersionUploadHistory();

        return app;
    }
}
