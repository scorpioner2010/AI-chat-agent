using System.Security.Cryptography;
using System.Text;
using AIChatAgent.Domain.Entities;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.ValueObjects;
using AIChatAgent.Infrastructure.Persistence.Entities;

namespace AIChatAgent.Infrastructure.Persistence;

internal static class PersistenceMapper
{
    public static CandidateProfile ToDomain(CandidateRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var candidate = new CandidateProfile(record.Id, record.DisplayName, record.Status);

        foreach (var signal in record.InterestSignals.OrderBy(signal => signal.Id))
        {
            candidate.AddInterestSignal(new InterestSignal(
                signal.Type,
                signal.Value,
                new InterestEvidence(
                    signal.EvidenceSourceText,
                    signal.EvidenceConfidence,
                    signal.EvidenceConsentState)));
        }

        return candidate;
    }

    public static Conversation ToDomain(ConversationRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (record.Candidate is null)
        {
            throw new InvalidOperationException(
                $"Conversation '{record.Id}' cannot be materialized without candidate '{record.CandidateId}'.");
        }

        var messages = record.Messages
            .OrderBy(message => message.Ordinal)
            .Select(message => new ConversationMessage(
                message.Author,
                message.Text,
                message.SentAt,
                message.Topic,
                message.ConsentState))
            .ToArray();

        var stoppedTopics = record.StoppedTopics
            .Select(topic => topic.Topic)
            .ToArray();

        return Conversation.Rehydrate(
            record.Id,
            ToDomain(record.Candidate),
            record.Stage,
            messages,
            stoppedTopics);
    }

    public static CandidateRecord ToRecord(CandidateProfile candidateProfile)
    {
        ArgumentNullException.ThrowIfNull(candidateProfile);

        var record = new CandidateRecord
        {
            Id = candidateProfile.Id,
            DisplayName = candidateProfile.DisplayName,
            Status = candidateProfile.Status
        };

        SyncCandidate(record, candidateProfile);

        return record;
    }

    public static ConversationRecord ToRecord(Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        EnsureUniqueMessages(conversation);

        var record = new ConversationRecord
        {
            Id = conversation.Id,
            CandidateId = conversation.CandidateProfile.Id,
            Stage = conversation.Stage
        };

        SyncConversation(record, conversation);

        return record;
    }

    public static void SyncCandidate(CandidateRecord record, CandidateProfile candidateProfile)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(candidateProfile);

        record.DisplayName = candidateProfile.DisplayName;
        record.Status = candidateProfile.Status;

        var desiredSignals = candidateProfile.InterestSignals
            .Select(ToRecord)
            .ToArray();

        SyncInterestSignals(record, desiredSignals);
    }

    public static void SyncConversation(ConversationRecord record, Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(conversation);

        EnsureUniqueMessages(conversation);

        record.CandidateId = conversation.CandidateProfile.Id;
        record.Stage = conversation.Stage;

        var desiredMessages = conversation.Messages
            .Select((message, ordinal) => ToRecord(message, ordinal))
            .ToArray();
        SyncMessages(record, desiredMessages);

        var desiredStoppedTopics = conversation.StoppedTopics
            .Select(topic => new StoppedTopicRecord
            {
                ConversationId = conversation.Id,
                Topic = topic
            })
            .ToArray();
        SyncStoppedTopics(record, desiredStoppedTopics);
    }

    public static string CreateMessageFingerprint(ConversationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return HashParts(
            message.Author.ToString(),
            message.Text,
            message.SentAt.ToUniversalTime().Ticks.ToString(),
            message.Topic?.ToString() ?? string.Empty,
            message.ConsentState.ToString());
    }

    private static InterestSignalRecord ToRecord(InterestSignal signal)
    {
        return new InterestSignalRecord
        {
            Type = signal.Type,
            Value = signal.Value,
            EvidenceSourceText = signal.Evidence.SourceText,
            EvidenceConfidence = signal.Evidence.Confidence,
            EvidenceConsentState = signal.Evidence.ConsentState,
            Fingerprint = CreateInterestSignalFingerprint(signal)
        };
    }

    private static ConversationMessageRecord ToRecord(ConversationMessage message, int ordinal)
    {
        return new ConversationMessageRecord
        {
            Ordinal = ordinal,
            Author = message.Author,
            Text = message.Text,
            SentAt = message.SentAt,
            Topic = message.Topic,
            ConsentState = message.ConsentState,
            Fingerprint = CreateMessageFingerprint(message)
        };
    }

    private static void SyncInterestSignals(
        CandidateRecord candidateRecord,
        IReadOnlyCollection<InterestSignalRecord> desiredSignals)
    {
        var desiredByFingerprint = desiredSignals.ToDictionary(signal => signal.Fingerprint);

        candidateRecord.InterestSignals.RemoveAll(signal => !desiredByFingerprint.ContainsKey(signal.Fingerprint));

        foreach (var desiredSignal in desiredSignals)
        {
            var existingSignal = candidateRecord.InterestSignals
                .FirstOrDefault(signal => signal.Fingerprint == desiredSignal.Fingerprint);

            if (existingSignal is null)
            {
                candidateRecord.InterestSignals.Add(desiredSignal);
                continue;
            }

            existingSignal.Type = desiredSignal.Type;
            existingSignal.Value = desiredSignal.Value;
            existingSignal.EvidenceSourceText = desiredSignal.EvidenceSourceText;
            existingSignal.EvidenceConfidence = desiredSignal.EvidenceConfidence;
            existingSignal.EvidenceConsentState = desiredSignal.EvidenceConsentState;
        }
    }

    private static void SyncMessages(
        ConversationRecord conversationRecord,
        IReadOnlyCollection<ConversationMessageRecord> desiredMessages)
    {
        var desiredByFingerprint = desiredMessages.ToDictionary(message => message.Fingerprint);

        conversationRecord.Messages.RemoveAll(message => !desiredByFingerprint.ContainsKey(message.Fingerprint));

        foreach (var desiredMessage in desiredMessages)
        {
            var existingMessage = conversationRecord.Messages
                .FirstOrDefault(message => message.Fingerprint == desiredMessage.Fingerprint);

            if (existingMessage is null)
            {
                conversationRecord.Messages.Add(desiredMessage);
                continue;
            }

            existingMessage.Ordinal = desiredMessage.Ordinal;
            existingMessage.Author = desiredMessage.Author;
            existingMessage.Text = desiredMessage.Text;
            existingMessage.SentAt = desiredMessage.SentAt;
            existingMessage.Topic = desiredMessage.Topic;
            existingMessage.ConsentState = desiredMessage.ConsentState;
        }
    }

    private static void SyncStoppedTopics(
        ConversationRecord conversationRecord,
        IReadOnlyCollection<StoppedTopicRecord> desiredStoppedTopics)
    {
        var desiredTopics = desiredStoppedTopics
            .Select(topic => topic.Topic)
            .ToHashSet();

        conversationRecord.StoppedTopics.RemoveAll(topic => !desiredTopics.Contains(topic.Topic));

        foreach (var desiredTopic in desiredStoppedTopics)
        {
            if (conversationRecord.StoppedTopics.Any(topic => topic.Topic == desiredTopic.Topic))
            {
                continue;
            }

            conversationRecord.StoppedTopics.Add(desiredTopic);
        }
    }

    private static void EnsureUniqueMessages(Conversation conversation)
    {
        var duplicates = conversation.Messages
            .GroupBy(CreateMessageFingerprint)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new InvalidOperationException(
                $"Conversation '{conversation.Id}' contains duplicate messages and cannot be persisted.");
        }
    }

    private static string CreateInterestSignalFingerprint(InterestSignal signal)
    {
        return HashParts(
            signal.Type.ToString(),
            signal.Value.ToString(),
            signal.Evidence.SourceText,
            signal.Evidence.Confidence.ToString("G"),
            signal.Evidence.ConsentState.ToString());
    }

    private static string HashParts(params string[] parts)
    {
        var input = string.Join('\u001F', parts);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(bytes);
    }
}
