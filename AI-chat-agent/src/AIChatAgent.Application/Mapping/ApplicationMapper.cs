using AIChatAgent.Application.DTOs;
using AIChatAgent.Domain.Entities;

namespace AIChatAgent.Application.Mapping;

internal static class ApplicationMapper
{
    public static CandidateSummary ToCandidateSummary(CandidateProfile candidateProfile)
    {
        ArgumentNullException.ThrowIfNull(candidateProfile);

        return new CandidateSummary(
            candidateProfile.Id,
            candidateProfile.DisplayName,
            candidateProfile.Status,
            candidateProfile.InterestSignals.Count);
    }

    public static ConversationSnapshot ToConversationSnapshot(Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        var messages = conversation.Messages
            .Select(message => new ConversationSnapshot.Message(
                message.Author,
                message.Text,
                message.SentAt,
                message.Topic,
                message.ConsentState))
            .ToArray();

        return new ConversationSnapshot(
            conversation.Id,
            ToCandidateSummary(conversation.CandidateProfile),
            conversation.Stage,
            messages,
            conversation.StoppedTopics.ToArray());
    }
}
