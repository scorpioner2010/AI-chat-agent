using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Infrastructure.Persistence.Entities;

internal sealed class ConversationRecord
{
    public string Id { get; set; } = string.Empty;

    public string CandidateId { get; set; } = string.Empty;

    public ConversationStage Stage { get; set; }

    public CandidateRecord? Candidate { get; set; }

    public List<ConversationMessageRecord> Messages { get; } = new();

    public List<StoppedTopicRecord> StoppedTopics { get; } = new();
}
