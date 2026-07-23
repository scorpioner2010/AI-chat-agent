using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.ValueObjects;

namespace AIChatAgent.Application.DTOs;

public sealed record AnalysisResult
{
    public AnalysisResult(
        CandidateSummary candidate,
        CompatibilityScore compatibilityScore,
        IReadOnlyCollection<InterestType> desiredInterests,
        bool shouldShortlist)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(desiredInterests);

        Candidate = candidate;
        CompatibilityScore = compatibilityScore;
        DesiredInterests = desiredInterests;
        ShouldShortlist = shouldShortlist;
    }

    public CandidateSummary Candidate { get; }

    public CompatibilityScore CompatibilityScore { get; }

    public IReadOnlyCollection<InterestType> DesiredInterests { get; }

    public bool ShouldShortlist { get; }
}
