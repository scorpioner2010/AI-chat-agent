using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Domain.Entities;

public sealed class Conversation
{
    private readonly List<ConversationMessage> _messages = new();
    private readonly HashSet<InterestType> _stoppedTopics = new();

    public Conversation(
        string id,
        CandidateProfile candidateProfile,
        ConversationStage stage = ConversationStage.NotStarted)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Conversation id is required.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(candidateProfile);

        Id = id;
        CandidateProfile = candidateProfile;
        Stage = stage;
    }

    public static Conversation Rehydrate(
        string id,
        CandidateProfile candidateProfile,
        ConversationStage stage,
        IEnumerable<ConversationMessage> messages,
        IEnumerable<InterestType> stoppedTopics)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(stoppedTopics);

        var conversation = new Conversation(id, candidateProfile, stage);

        conversation._messages.AddRange(messages);

        foreach (var stoppedTopic in stoppedTopics)
        {
            if (stoppedTopic == InterestType.Unknown)
            {
                throw new ArgumentException("A stopped topic must be specific.", nameof(stoppedTopics));
            }

            conversation._stoppedTopics.Add(stoppedTopic);
        }

        return conversation;
    }

    public string Id { get; }

    public CandidateProfile CandidateProfile { get; }

    public ConversationStage Stage { get; private set; }

    public IReadOnlyList<ConversationMessage> Messages => _messages.AsReadOnly();

    public IReadOnlySet<InterestType> StoppedTopics => _stoppedTopics;

    public ConversationMessage? LastMessage => _messages.Count == 0 ? null : _messages[^1];

    public bool IsTopicStopped(InterestType topic)
    {
        return _stoppedTopics.Contains(topic);
    }

    internal void AddMessage(ConversationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _messages.Add(message);
    }

    internal void SetStage(ConversationStage stage)
    {
        Stage = stage;
    }

    internal void StopTopic(InterestType topic)
    {
        if (topic == InterestType.Unknown)
        {
            throw new ArgumentException("A stopped topic must be specific.", nameof(topic));
        }

        _stoppedTopics.Add(topic);
    }
}
