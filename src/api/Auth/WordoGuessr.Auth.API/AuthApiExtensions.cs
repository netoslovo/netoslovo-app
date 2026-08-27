using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;
using WordoGuessr.Auth.API.Security;
using WordoGuessr.Auth.API.UseCases.Admin;
using WordoGuessr.Auth.API.UseCases.User.ChangeUserName;
using WordoGuessr.Auth.API.UseCases.User.GetCurrentAuthState;
using WordoGuessr.Auth.API.UseCases.User.GetProfile;
using WordoGuessr.Auth.API.UseCases.User.Logout;
using WordoGuessr.Auth.API.UseCases.User.RequestEmailOtp;
using WordoGuessr.Auth.API.UseCases.User.SaveOneTimeUserNoticeView;
using WordoGuessr.Auth.API.UseCases.User.SaveRecurringUserNoticeView;
using WordoGuessr.Auth.API.UseCases.User.ShouldShowUserNotice;
using WordoGuessr.Auth.API.UseCases.User.VerifyEmailOtp;

namespace WordoGuessr.Auth.API;

public static class AuthApiExtensions
{
    public static IServiceCollection AddAuthApi(this IServiceCollection services)
    {
        services.AddTransient<IModuleClaimsTransformation, AuthPermissionClaimsTransformation>();
        services.AddAuthorization(options => options.AddAuthPolicies());

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/auth")
            .WithTags("Auth")
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests));

        group
            .MapRequestEmailOtp()
            .MapVerifyEmailOtp()
            .MapGetCurrentAuthState()
            .MapGetProfile()
            .MapLogout()
            .MapChangeUserName()
            .MapShouldShowUserNotice()
            .MapSaveOneTimeUserNoticeView()
            .MapSaveRecurringUserNoticeView();


        var adminGroup = group
            .MapGroup("/admin")
            .RequireAuthorization()
            .WithMetadata(
                new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status401Unauthorized),
                new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status403Forbidden));

        adminGroup
            .MapPublishUserNameFilterVersion()
            .MapGetCurrentUserNameFilterVersion();

        return app;
    }
}
