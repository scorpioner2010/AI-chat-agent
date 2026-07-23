using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Infrastructure.Persistence.Entities;

internal sealed class StoppedTopicRecord
{
    public string ConversationId { get; set; } = string.Empty;

    public InterestType Topic { get; set; }

    public ConversationRecord? Conversation { get; set; }
}
