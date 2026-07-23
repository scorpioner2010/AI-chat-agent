using AIChatAgent.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIChatAgent.Infrastructure.Persistence;

public sealed class AgentDbContext : DbContext
{
    public AgentDbContext(DbContextOptions<AgentDbContext> options)
        : base(options)
    {
    }

    internal DbSet<CandidateRecord> Candidates => Set<CandidateRecord>();

    internal DbSet<ConversationRecord> Conversations => Set<ConversationRecord>();

    internal DbSet<ConversationMessageRecord> ConversationMessages => Set<ConversationMessageRecord>();

    internal DbSet<InterestSignalRecord> InterestSignals => Set<InterestSignalRecord>();

    internal DbSet<StoppedTopicRecord> StoppedTopics => Set<StoppedTopicRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentDbContext).Assembly);
    }
}
