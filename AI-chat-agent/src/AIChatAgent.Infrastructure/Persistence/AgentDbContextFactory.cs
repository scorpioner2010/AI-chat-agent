using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AIChatAgent.Infrastructure.Persistence;

public sealed class AgentDbContextFactory : IDesignTimeDbContextFactory<AgentDbContext>
{
    public AgentDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
            ? args[0]
            : "Data Source=agent.db";

        var options = new DbContextOptionsBuilder<AgentDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new AgentDbContext(options);
    }
}
