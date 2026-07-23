using AIChatAgent.Application.Abstractions;
using AIChatAgent.Domain.Entities;
using AIChatAgent.Infrastructure.Persistence;
using AIChatAgent.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIChatAgent.Infrastructure.Repositories;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly AgentDbContext _dbContext;

    public ConversationRepository(AgentDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Conversation?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        EnsureRequired(id, nameof(id), "Conversation id is required.");

        var record = await QueryConversations()
            .AsNoTracking()
            .FirstOrDefaultAsync(conversation => conversation.Id == id, cancellationToken);

        return record is null ? null : PersistenceMapper.ToDomain(record);
    }

    public async Task<Conversation?> GetByCandidateIdAsync(
        string candidateId,
        CancellationToken cancellationToken)
    {
        EnsureRequired(candidateId, nameof(candidateId), "Candidate id is required.");

        var record = await QueryConversations()
            .AsNoTracking()
            .OrderBy(conversation => conversation.Id)
            .FirstOrDefaultAsync(conversation => conversation.CandidateId == candidateId, cancellationToken);

        return record is null ? null : PersistenceMapper.ToDomain(record);
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        var exists = await _dbContext.Conversations
            .AnyAsync(record => record.Id == conversation.Id, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Conversation '{conversation.Id}' already exists.");
        }

        await UpsertCandidateAsync(conversation.CandidateProfile, cancellationToken);

        _dbContext.Conversations.Add(PersistenceMapper.ToRecord(conversation));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        await UpsertCandidateAsync(conversation.CandidateProfile, cancellationToken);

        var record = await _dbContext.Conversations
            .Include(conversationRecord => conversationRecord.Messages)
            .Include(conversationRecord => conversationRecord.StoppedTopics)
            .FirstOrDefaultAsync(conversationRecord => conversationRecord.Id == conversation.Id, cancellationToken);

        if (record is null)
        {
            _dbContext.Conversations.Add(PersistenceMapper.ToRecord(conversation));
        }
        else
        {
            PersistenceMapper.SyncConversation(record, conversation);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ConversationRecord> QueryConversations()
    {
        return _dbContext.Conversations
            .Include(conversation => conversation.Candidate)
            .ThenInclude(candidate => candidate!.InterestSignals)
            .Include(conversation => conversation.Messages)
            .Include(conversation => conversation.StoppedTopics);
    }

    private async Task UpsertCandidateAsync(
        CandidateProfile candidateProfile,
        CancellationToken cancellationToken)
    {
        var existingCandidate = await _dbContext.Candidates
            .Include(candidate => candidate.InterestSignals)
            .FirstOrDefaultAsync(candidate => candidate.Id == candidateProfile.Id, cancellationToken);

        if (existingCandidate is null)
        {
            _dbContext.Candidates.Add(PersistenceMapper.ToRecord(candidateProfile));
            return;
        }

        PersistenceMapper.SyncCandidate(existingCandidate, candidateProfile);
    }

    private static void EnsureRequired(string value, string parameterName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
