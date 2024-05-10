using Microsoft.Extensions.Logging;

namespace SqlLiteExplorer;

public class DbContextFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public DbContextFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public DbContext CreateDbContext(string connectionString) 
    {
        return new DbContext() { ConnectionString = connectionString};
    }
}
