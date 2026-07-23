using AIChatAgent.Application.Abstractions;
using AIChatAgent.Application.DTOs;
using AIChatAgent.Application.Mapping;
using AIChatAgent.Domain.Entities;

namespace AIChatAgent.Application.UseCases;

public sealed class RegisterCandidate
{
    private readonly ICandidateRepository _candidateRepository;

    public RegisterCandidate(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository
            ?? throw new ArgumentNullException(nameof(candidateRepository));
    }

    public async Task<CandidateSummary> ExecuteAsync(
        string id,
        string displayName,
        CancellationToken cancellationToken)
    {
        EnsureRequired(id, nameof(id), "Candidate id is required.");
        EnsureRequired(displayName, nameof(displayName), "Candidate display name is required.");

        var existingCandidate = await _candidateRepository.GetByIdAsync(id, cancellationToken);
        if (existingCandidate is not null)
        {
            throw new InvalidOperationException($"Candidate '{id}' is already registered.");
        }

        var candidate = new CandidateProfile(id, displayName);

        await _candidateRepository.AddAsync(candidate, cancellationToken);

        return ApplicationMapper.ToCandidateSummary(candidate);
    }

    private static void EnsureRequired(string value, string parameterName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
