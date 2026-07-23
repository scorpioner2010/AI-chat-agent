using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Domain.ValueObjects;

public sealed record InterestEvidence
{
    public InterestEvidence(
        string sourceText,
        decimal confidence = 1m,
        ConsentState consentState = ConsentState.Unknown)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            throw new ArgumentException("Interest evidence source text is required.", nameof(sourceText));
        }

        if (confidence is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidence),
                confidence,
                "Interest evidence confidence must be between 0 and 1.");
        }

        SourceText = sourceText;
        Confidence = confidence;
        ConsentState = consentState;
    }

    public string SourceText { get; }

    public decimal Confidence { get; }

    public ConsentState ConsentState { get; }
}
