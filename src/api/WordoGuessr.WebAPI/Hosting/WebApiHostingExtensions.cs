using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using WordoGuessr.WebAPI.CurrentUser;
using WordoGuessr.WebAPI.Validation;

namespace WordoGuessr.WebAPI.Hosting;

internal static class WebApiHostingExtensions
{
    public static IHostApplicationBuilder AddWebApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddValidationExceptionHandlers();
        builder.Services.AddExceptionHandler<CurrentUserUnauthorizedExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddHealthChecks();

        if (builder.Configuration.IsHostingFeatureEnabled(d => d.ForwardedHeaders))
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto |
                    ForwardedHeaders.XForwardedHost;
            });
        }

        if (builder.Configuration.IsHostingFeatureEnabled(d => d.OpenApi))
        {
            builder.Services.AddOpenApi();
        }

        return builder;
    }
}
