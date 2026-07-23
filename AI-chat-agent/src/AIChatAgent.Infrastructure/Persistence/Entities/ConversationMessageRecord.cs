using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Infrastructure.Persistence.Entities;

internal sealed class ConversationMessageRecord
{
    public int Id { get; set; }

    public string ConversationId { get; set; } = string.Empty;

    public int Ordinal { get; set; }

    public MessageAuthor Author { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTimeOffset SentAt { get; set; }

    public InterestType? Topic { get; set; }

    public ConsentState ConsentState { get; set; }

    public string Fingerprint { get; set; } = string.Empty;

    public ConversationRecord? Conversation { get; set; }
}
