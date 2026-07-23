using Microsoft.EntityFrameworkCore;

namespace AIChatAgent.Infrastructure.Persistence;

public sealed class AgentDatabaseInitializer
{
    private readonly AgentDbContext _dbContext;

    public AgentDatabaseInitializer(AgentDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.GetMigrations().Any())
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
            return;
        }

        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
