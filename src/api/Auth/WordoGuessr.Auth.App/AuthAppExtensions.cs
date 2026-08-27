using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Dto;
using WordoGuessr.Auth.App.Services;
using WordoGuessr.Auth.App.UseCases.Admin.GetCurrentUserNameFilterVersion;
using WordoGuessr.Auth.App.UseCases.Admin.PublishNewUserNameFilterVersion;
using WordoGuessr.Auth.App.UseCases.User.ChangeUserName;
using WordoGuessr.Auth.App.UseCases.User.GetCurrentAuthState;
using WordoGuessr.Auth.App.UseCases.User.GetProfile;
using WordoGuessr.Auth.App.UseCases.User.Logout;
using WordoGuessr.Auth.App.UseCases.User.RequestEmailOtp;
using WordoGuessr.Auth.App.UseCases.User.SaveOneTimeUserNoticeView;
using WordoGuessr.Auth.App.UseCases.User.SaveRecurringUserNoticeView;
using WordoGuessr.Auth.App.UseCases.User.ShouldShowUserNotice;
using WordoGuessr.Auth.App.UseCases.User.VerifyEmailOtp;
using WordoGuessr.Auth.Contract;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.App;

public static class AuthAppExtensions
{
    public static IServiceCollection AddAuthApp(this IServiceCollection services)
    {
        services.AddTransient<IAuthModule, AuthModule>();

        services.AddValidatorsFromAssemblyContaining<RequestEmailOtpCommand>();

        services.AddOptions<AuthOtpOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IOtpCodeGenerator, NumericOtpCodeGenerator>();
        services.AddSingleton<IUserNameGenerator, RandomUserNameGenerator>();
        services.AddSingleton<AuthAppMetrics>();

        services.AddCommandHandler<RequestEmailOtpHandler, RequestEmailOtpCommand, Result<Guid, RequestEmailOtpError>>();
        services.AddCommandHandler<VerifyEmailOtpHandler, VerifyEmailOtpCommand, Result<VerifyEmailOtpError>>();
        services.AddCommandHandler<LogoutHandler, LogoutCommand>();
        services.AddCommandHandler<ChangeUserNameHandler, ChangeUserNameCommand, Result<ChangeUserNameError>>();
        services.AddCommandHandler<PublishNewUserNameFilterVersionHandler, PublishNewUserNameFilterVersionCommand>();
        services.AddCommandHandler<SaveOneTimeUserNoticeViewHandler, SaveOneTimeUserNoticeViewCommand>();
        services.AddCommandHandler<SaveRecurringUserNoticeViewHandler, SaveRecurringUserNoticeViewCommand>();

        services.AddQueryHandler<GetCurrentAuthStateHandler, GetCurrentAuthStateQuery, AuthMeDto>();
        services.AddQueryHandler<GetProfileHandler, GetProfileQuery, ProfileDto?>();
        services.AddQueryHandler<GetCurrentUserNameFilterVersionHandler, GetCurrentUserNameFilterVersionQuery, int>();
        services.AddQueryHandler<ShouldShowUserNoticeHandler, ShouldShowUserNoticeQuery, bool>();

        return services;
    }
}
