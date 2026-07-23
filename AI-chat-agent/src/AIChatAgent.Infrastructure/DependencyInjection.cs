using AIChatAgent.Application.Abstractions;
using AIChatAgent.Infrastructure.Persistence;
using AIChatAgent.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AIChatAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAiChatAgentInfrastructure(
        this IServiceCollection services,
        string sqliteConnectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(sqliteConnectionString))
        {
            throw new ArgumentException("SQLite connection string is required.", nameof(sqliteConnectionString));
        }

        services.AddDbContext<AgentDbContext>(options =>
            options.UseSqlite(sqliteConnectionString));

        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<AgentDatabaseInitializer>();

        return services;
    }
}
