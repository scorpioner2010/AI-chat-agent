using AIChatAgent.Application.Abstractions;
using AIChatAgent.Application.DTOs;
using AIChatAgent.Application.Mapping;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.Services;
using AIChatAgent.Domain.ValueObjects;

namespace AIChatAgent.Application.UseCases;

public sealed class EvaluateCandidate
{
    public const int DefaultShortlistThreshold = 70;

    private readonly ICandidateRepository _candidateRepository;
    private readonly CompatibilityScorer _compatibilityScorer;

    public EvaluateCandidate(
        ICandidateRepository candidateRepository,
        CompatibilityScorer compatibilityScorer)
    {
        _candidateRepository = candidateRepository
            ?? throw new ArgumentNullException(nameof(candidateRepository));
        _compatibilityScorer = compatibilityScorer
            ?? throw new ArgumentNullException(nameof(compatibilityScorer));
    }

    public async Task<AnalysisResult> ExecuteAsync(
        string candidateId,
        IReadOnlyCollection<InterestType> desiredInterests,
        CancellationToken cancellationToken,
        int shortlistThreshold = DefaultShortlistThreshold)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            throw new ArgumentException("Candidate id is required.", nameof(candidateId));
        }

        ArgumentNullException.ThrowIfNull(desiredInterests);

        var threshold = new CompatibilityScore(shortlistThreshold);
        var candidate = await _candidateRepository.GetByIdAsync(candidateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Candidate '{candidateId}' was not found.");

        var normalizedDesiredInterests = desiredInterests
            .Distinct()
            .ToArray();
        var compatibilityScore = _compatibilityScorer.Score(candidate, normalizedDesiredInterests);

        return new AnalysisResult(
            ApplicationMapper.ToCandidateSummary(candidate),
            compatibilityScore,
            normalizedDesiredInterests,
            compatibilityScore.Value >= threshold.Value);
    }
}
