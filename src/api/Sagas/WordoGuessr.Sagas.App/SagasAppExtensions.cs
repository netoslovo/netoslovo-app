using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Sagas.App.UseCases.CancelWordsVersionUpload;
using WordoGuessr.Sagas.App.UseCases.GetWordsVersionUpload;
using WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadFromHistory;
using WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadHistory;
using WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploads;
using WordoGuessr.Sagas.App.UseCases.UploadWordsVersion;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App;

public static class SagasAppExtensions
{
    public static IServiceCollection AddSagasApp(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GetWordsVersionUploadHistoryValidator>(includeInternalTypes: true);

        services
            .AddCommandHandler<UploadWordsVersionHandler, UploadWordsVersionCommand, Result<UploadWordsVersionError>>()
            .AddCommandHandler<CancelWordsVersionUploadHandler, CancelWordsVersionUploadCommand, Result<CancelWordsVersionUploadError>>();

        services
            .AddQueryHandler<GetWordsVersionUploadHandler, GetWordsVersionUploadQuery, UploadWordsVersionSagaDto?>()
            .AddQueryHandler<GetWordsVersionUploadFromHistoryHandler, GetWordsVersionUploadFromHistoryQuery, UploadWordsVersionSagaHistoryDto?>()
            .AddQueryHandler<GetWordsVersionUploadHistoryHandler, GetWordsVersionUploadHistoryQuery, UploadWordsVersionSagaHistoryPageDto>()
            .AddQueryHandler<GetWordsVersionUploadsHandler, GetWordsVersionUploadsQuery, UploadWordsVersionSagaPageDto>();

        return services;
    }
}
