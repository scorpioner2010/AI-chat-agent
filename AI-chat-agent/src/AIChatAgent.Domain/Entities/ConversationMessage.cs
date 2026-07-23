using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Domain.Entities;

public sealed class ConversationMessage
{
    public ConversationMessage(
        MessageAuthor author,
        string text,
        DateTimeOffset sentAt,
        InterestType? topic = null,
        ConsentState consentState = ConsentState.Unknown)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Conversation message text is required.", nameof(text));
        }

        if (topic == InterestType.Unknown)
        {
            throw new ArgumentException("Use null when the message topic is unknown.", nameof(topic));
        }

        Author = author;
        Text = text;
        SentAt = sentAt;
        Topic = topic;
        ConsentState = consentState;
    }

    public MessageAuthor Author { get; }

    public string Text { get; }

    public DateTimeOffset SentAt { get; }

    public InterestType? Topic { get; }

    public ConsentState ConsentState { get; }

    public bool IsClearRefusal => ConsentState == ConsentState.Refused;
}
