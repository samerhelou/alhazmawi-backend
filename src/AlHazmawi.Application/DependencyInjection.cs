using Microsoft.Extensions.DependencyInjection;

namespace AlHazmawi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation & other application services can be registered here
        return services;
    }
}
