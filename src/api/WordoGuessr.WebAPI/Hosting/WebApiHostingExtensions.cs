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

        var forwardedHeadersFeature = builder.Configuration.GetForwardedHeadersFeature();
        if (forwardedHeadersFeature.Enabled)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardLimit = forwardedHeadersFeature.ForwardLimit;
                foreach (var network in forwardedHeadersFeature.GetKnownIPNetworks())
                {
                    options.KnownIPNetworks.Add(network);
                }

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
