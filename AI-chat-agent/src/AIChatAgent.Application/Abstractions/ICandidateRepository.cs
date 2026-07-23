using AIChatAgent.Domain.Entities;

namespace AIChatAgent.Application.Abstractions;

public interface ICandidateRepository
{
    Task<CandidateProfile?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task AddAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken);

    Task UpdateAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken);
}
