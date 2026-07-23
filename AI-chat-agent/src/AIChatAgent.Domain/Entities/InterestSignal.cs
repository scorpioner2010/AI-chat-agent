using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.ValueObjects;

namespace AIChatAgent.Domain.Entities;

public sealed class InterestSignal
{
    public InterestSignal(InterestType type, SignalValue value, InterestEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        Type = type;
        Value = value;
        Evidence = evidence;
    }

    public InterestType Type { get; }

    public SignalValue Value { get; }

    public InterestEvidence Evidence { get; }

    public bool IsExplicitPositive => Value == SignalValue.Positive;

    public bool IsExplicitNegative => Value is SignalValue.Negative or SignalValue.Refused;
}
