using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Application.DTOs;

public sealed record ConversationSnapshot
{
    public ConversationSnapshot(
        string id,
        CandidateSummary candidate,
        ConversationStage stage,
        IReadOnlyList<Message> messages,
        IReadOnlyCollection<InterestType> stoppedTopics)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Conversation snapshot id is required.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(stoppedTopics);

        Id = id;
        Candidate = candidate;
        Stage = stage;
        Messages = messages;
        StoppedTopics = stoppedTopics;
    }

    public string Id { get; }

    public CandidateSummary Candidate { get; }

    public ConversationStage Stage { get; }

    public IReadOnlyList<Message> Messages { get; }

    public IReadOnlyCollection<InterestType> StoppedTopics { get; }

    public sealed record Message
    {
        public Message(
            MessageAuthor author,
            string text,
            DateTimeOffset sentAt,
            InterestType? topic,
            ConsentState consentState)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Conversation snapshot message text is required.", nameof(text));
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
    }
}
