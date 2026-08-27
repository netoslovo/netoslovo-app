namespace WordoGuessr.WebAPI.Validation;

internal static class ValidationServiceCollectionExtensions
{
    public static IServiceCollection AddValidationExceptionHandlers(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });

        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<BadHttpRequestExceptionHandler>();

        return services;
    }
}
