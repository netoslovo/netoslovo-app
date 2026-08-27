using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace WordoGuessr.API.BuildingBlocks.CQRS;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandHandler<THandler, TCommand>(
        this IServiceCollection services)
        where THandler : class, ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(serviceProvider =>
            new ValidationCommandHandlerDecorator<TCommand>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetServices<IValidator<TCommand>>()));

        return services;
    }

    public static IServiceCollection AddCommandHandler<THandler, TCommand, TResult>(
        this IServiceCollection services)
        where THandler : class, ICommandHandler<TCommand, TResult>
        where TCommand : class, ICommand<TResult>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new ValidationCommandHandlerDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetServices<IValidator<TCommand>>()));

        return services;
    }


    public static IServiceCollection AddQueryHandler<THandler, TQuery, TResult>(
        this IServiceCollection services)
        where THandler : class, IQueryHandler<TQuery, TResult>
        where TQuery : class, IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new ValidationQueryHandlerDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetServices<IValidator<TQuery>>()));

        return services;
    }
}
