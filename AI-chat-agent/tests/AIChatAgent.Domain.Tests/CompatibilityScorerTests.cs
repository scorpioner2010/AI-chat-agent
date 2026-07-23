using AIChatAgent.Domain.Entities;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.Services;
using AIChatAgent.Domain.ValueObjects;

namespace AIChatAgent.Domain.Tests;

public sealed class CompatibilityScorerTests
{
    private readonly CompatibilityScorer _scorer = new();

    [Fact]
    public void Unknown_interest_is_neutral_not_negative()
    {
        var candidate = CreateCandidateWithSignal(InterestType.Horror, SignalValue.Unknown);

        var score = _scorer.Score(candidate, [InterestType.Horror]);

        Assert.Equal(CompatibilityScore.NeutralValue, score.Value);
    }

    [Fact]
    public void Horror_interest_does_not_imply_interest_in_violence()
    {
        var candidate = CreateCandidateWithSignal(InterestType.Horror, SignalValue.Positive);

        var score = _scorer.Score(candidate, [InterestType.Violence]);

        Assert.Equal(CompatibilityScore.NeutralValue, score.Value);
    }

    [Fact]
    public void Knowing_mortal_kombat_does_not_imply_liking_it()
    {
        var candidate = CreateCandidateWithSignal(InterestType.MortalKombat, SignalValue.Known);

        var score = _scorer.Score(candidate, [InterestType.MortalKombat]);

        Assert.Equal(CompatibilityScore.NeutralValue, score.Value);
    }

    [Fact]
    public void Explicit_matching_positive_interest_increases_score()
    {
        var candidate = CreateCandidateWithSignal(InterestType.Horror, SignalValue.Positive);

        var score = _scorer.Score(candidate, [InterestType.Horror]);

        Assert.True(score.Value > CompatibilityScore.NeutralValue);
    }

    [Fact]
    public void Explicit_matching_negative_interest_decreases_score()
    {
        var candidate = CreateCandidateWithSignal(InterestType.Horror, SignalValue.Negative);

        var score = _scorer.Score(candidate, [InterestType.Horror]);

        Assert.True(score.Value < CompatibilityScore.NeutralValue);
    }

    private static CandidateProfile CreateCandidateWithSignal(InterestType type, SignalValue value)
    {
        var candidate = new CandidateProfile("candidate-1", "Candidate");
        candidate.AddInterestSignal(new InterestSignal(
            type,
            value,
            new InterestEvidence("explicit profile or conversation evidence")));

        return candidate;
    }
}
