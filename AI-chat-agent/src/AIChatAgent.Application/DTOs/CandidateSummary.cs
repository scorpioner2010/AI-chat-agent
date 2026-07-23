using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Application.DTOs;

public sealed record CandidateSummary
{
    public CandidateSummary(
        string id,
        string displayName,
        CandidateStatus status,
        int interestSignalCount)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Candidate summary id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Candidate summary display name is required.", nameof(displayName));
        }

        if (interestSignalCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interestSignalCount),
                interestSignalCount,
                "Interest signal count cannot be negative.");
        }

        Id = id;
        DisplayName = displayName;
        Status = status;
        InterestSignalCount = interestSignalCount;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public CandidateStatus Status { get; }

    public int InterestSignalCount { get; }
}
