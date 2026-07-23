namespace AIChatAgent.Domain.ValueObjects;

public readonly record struct CompatibilityScore
{
    public const int MinValue = 0;
    public const int NeutralValue = 50;
    public const int MaxValue = 100;

    public CompatibilityScore(int value)
    {
        if (value is < MinValue or > MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Compatibility score must be between {MinValue} and {MaxValue}.");
        }

        Value = value;
    }

    public int Value { get; }

    public static CompatibilityScore Neutral => new(NeutralValue);

    public static CompatibilityScore Clamp(int value)
    {
        return new CompatibilityScore(Math.Clamp(value, MinValue, MaxValue));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
