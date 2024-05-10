using Microsoft.Extensions.DependencyInjection;

namespace SqlLiteExplorer;

public static class DependencyInjection
{
    public static IServiceCollection AddSqliteDbContext(this IServiceCollection services)
    {
        services
            .AddScoped<DbContextFactory>()
            ;
        return services;
    }
}
