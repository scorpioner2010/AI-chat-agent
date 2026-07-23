using AIChatAgent.Application.Abstractions;
using AIChatAgent.Domain.Entities;
using AIChatAgent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIChatAgent.Infrastructure.Repositories;

public sealed class CandidateRepository : ICandidateRepository
{
    private readonly AgentDbContext _dbContext;

    public CandidateRepository(AgentDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<CandidateProfile?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        EnsureRequired(id, nameof(id), "Candidate id is required.");

        var record = await _dbContext.Candidates
            .AsNoTracking()
            .Include(candidate => candidate.InterestSignals)
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        return record is null ? null : PersistenceMapper.ToDomain(record);
    }

    public async Task AddAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateProfile);

        var exists = await _dbContext.Candidates
            .AnyAsync(candidate => candidate.Id == candidateProfile.Id, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Candidate '{candidateProfile.Id}' already exists.");
        }

        _dbContext.Candidates.Add(PersistenceMapper.ToRecord(candidateProfile));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateProfile);

        var record = await _dbContext.Candidates
            .Include(candidate => candidate.InterestSignals)
            .FirstOrDefaultAsync(candidate => candidate.Id == candidateProfile.Id, cancellationToken);

        if (record is null)
        {
            _dbContext.Candidates.Add(PersistenceMapper.ToRecord(candidateProfile));
        }
        else
        {
            PersistenceMapper.SyncCandidate(record, candidateProfile);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureRequired(string value, string parameterName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
