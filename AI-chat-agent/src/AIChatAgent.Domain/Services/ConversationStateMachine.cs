using AIChatAgent.Domain.Entities;
using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Domain.Services;

public sealed class ConversationStateMachine
{
    private static readonly IReadOnlyDictionary<ConversationStage, ConversationStage[]> ValidTransitions =
        new Dictionary<ConversationStage, ConversationStage[]>
        {
            [ConversationStage.NotStarted] =
            [
                ConversationStage.AwaitingCandidateReply,
                ConversationStage.CandidateReplied,
                ConversationStage.Closed
            ],
            [ConversationStage.AwaitingCandidateReply] =
            [
                ConversationStage.CandidateReplied,
                ConversationStage.Closed
            ],
            [ConversationStage.CandidateReplied] =
            [
                ConversationStage.AwaitingCandidateReply,
                ConversationStage.TopicStopped,
                ConversationStage.Qualified,
                ConversationStage.Closed
            ],
            [ConversationStage.TopicStopped] =
            [
                ConversationStage.AwaitingCandidateReply,
                ConversationStage.CandidateReplied,
                ConversationStage.Qualified,
                ConversationStage.Closed
            ],
            [ConversationStage.Qualified] =
            [
                ConversationStage.Closed
            ],
            [ConversationStage.Closed] = []
        };

    public bool CanTransition(ConversationStage currentStage, ConversationStage nextStage)
    {
        return ValidTransitions.TryGetValue(currentStage, out var validNextStages)
            && validNextStages.Contains(nextStage);
    }

    public void TransitionTo(Conversation conversation, ConversationStage nextStage)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        if (!CanTransition(conversation.Stage, nextStage))
        {
            throw new InvalidOperationException(
                $"Cannot transition conversation from {conversation.Stage} to {nextStage}.");
        }

        conversation.SetStage(nextStage);

        if (nextStage == ConversationStage.Qualified)
        {
            conversation.CandidateProfile.MarkQualified();
            return;
        }

        if (nextStage is ConversationStage.AwaitingCandidateReply
            or ConversationStage.CandidateReplied
            or ConversationStage.TopicStopped)
        {
            conversation.CandidateProfile.MarkActive();
        }
    }

    public void RecordMessage(Conversation conversation, ConversationMessage message)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(message);

        if (message.Author is MessageAuthor.Agent or MessageAuthor.User
            && conversation.Stage == ConversationStage.AwaitingCandidateReply)
        {
            throw new InvalidOperationException("Cannot initiate another message until the candidate replies.");
        }

        var nextStage = GetStageAfterMessage(message);

        if (nextStage.HasValue && conversation.Stage != nextStage.Value)
        {
            TransitionTo(conversation, nextStage.Value);
        }

        conversation.AddMessage(message);

        if (message.Author == MessageAuthor.Candidate
            && message.IsClearRefusal
            && message.Topic.HasValue)
        {
            StopTopic(conversation, message.Topic.Value);
        }
    }

    public void StopTopic(Conversation conversation, InterestType topic)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        conversation.StopTopic(topic);

        if (conversation.Stage != ConversationStage.TopicStopped)
        {
            TransitionTo(conversation, ConversationStage.TopicStopped);
        }
    }

    public bool CanDiscussTopic(Conversation conversation, InterestType topic)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        return !conversation.IsTopicStopped(topic);
    }

    public bool CanSendAutomaticMessage(Conversation conversation, InterestType? topic = null)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        if (conversation.CandidateProfile.Status is CandidateStatus.Qualified
            or CandidateStatus.Rejected
            or CandidateStatus.Archived)
        {
            return false;
        }

        if (conversation.Stage is ConversationStage.AwaitingCandidateReply
            or ConversationStage.Qualified
            or ConversationStage.Closed)
        {
            return false;
        }

        if (topic.HasValue && conversation.IsTopicStopped(topic.Value))
        {
            return false;
        }

        return true;
    }

    private static ConversationStage? GetStageAfterMessage(ConversationMessage message)
    {
        return message.Author switch
        {
            MessageAuthor.Agent or MessageAuthor.User => ConversationStage.AwaitingCandidateReply,
            MessageAuthor.Candidate => ConversationStage.CandidateReplied,
            MessageAuthor.System => null,
            _ => throw new ArgumentOutOfRangeException(nameof(message), message.Author, "Unknown message author.")
        };
    }
}
