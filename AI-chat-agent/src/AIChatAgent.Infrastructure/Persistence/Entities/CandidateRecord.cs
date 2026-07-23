using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Infrastructure.Persistence.Entities;

internal sealed class CandidateRecord
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public CandidateStatus Status { get; set; }

    public List<InterestSignalRecord> InterestSignals { get; } = new();

    public List<ConversationRecord> Conversations { get; } = new();
}
