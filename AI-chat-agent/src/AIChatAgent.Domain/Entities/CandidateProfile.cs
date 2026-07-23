using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Domain.Entities;

public sealed class CandidateProfile
{
    private readonly List<InterestSignal> _interestSignals = new();

    public CandidateProfile(string id, string displayName, CandidateStatus status = CandidateStatus.New)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Candidate profile id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Candidate display name is required.", nameof(displayName));
        }

        Id = id;
        DisplayName = displayName;
        Status = status;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public CandidateStatus Status { get; private set; }

    public IReadOnlyCollection<InterestSignal> InterestSignals => _interestSignals.AsReadOnly();

    public void AddInterestSignal(InterestSignal signal)
    {
        ArgumentNullException.ThrowIfNull(signal);

        _interestSignals.Add(signal);
    }

    public void MarkActive()
    {
        if (Status == CandidateStatus.New)
        {
            Status = CandidateStatus.Active;
        }
    }

    public void MarkQualified()
    {
        Status = CandidateStatus.Qualified;
    }

    public void Reject()
    {
        Status = CandidateStatus.Rejected;
    }
}
