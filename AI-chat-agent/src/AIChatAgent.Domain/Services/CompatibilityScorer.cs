using AIChatAgent.Domain.Entities;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.ValueObjects;

namespace AIChatAgent.Domain.Services;

public sealed class CompatibilityScorer
{
    private const int SignalWeight = 20;

    public CompatibilityScore Score(CandidateProfile candidateProfile, IEnumerable<InterestType> desiredInterests)
    {
        ArgumentNullException.ThrowIfNull(candidateProfile);
        ArgumentNullException.ThrowIfNull(desiredInterests);

        var score = CompatibilityScore.NeutralValue;
        var explicitInterests = desiredInterests
            .Where(interest => interest != InterestType.Unknown)
            .Distinct();

        foreach (var desiredInterest in explicitInterests)
        {
            var signals = candidateProfile.InterestSignals
                .Where(signal => signal.Type == desiredInterest)
                .ToArray();

            if (signals.Any(signal => signal.IsExplicitNegative))
            {
                score -= SignalWeight;
                continue;
            }

            if (signals.Any(signal => signal.IsExplicitPositive))
            {
                score += SignalWeight;
            }
        }

        return CompatibilityScore.Clamp(score);
    }
}
