using AIChatAgent.Domain.Entities;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.Services;

namespace AIChatAgent.Domain.Tests;

public sealed class ConversationStateMachineTests
{
    private static readonly (ConversationStage From, ConversationStage To)[] ValidTransitions =
    [
        (ConversationStage.NotStarted, ConversationStage.AwaitingCandidateReply),
        (ConversationStage.NotStarted, ConversationStage.CandidateReplied),
        (ConversationStage.NotStarted, ConversationStage.Closed),
        (ConversationStage.AwaitingCandidateReply, ConversationStage.CandidateReplied),
        (ConversationStage.AwaitingCandidateReply, ConversationStage.Closed),
        (ConversationStage.CandidateReplied, ConversationStage.AwaitingCandidateReply),
        (ConversationStage.CandidateReplied, ConversationStage.TopicStopped),
        (ConversationStage.CandidateReplied, ConversationStage.Qualified),
        (ConversationStage.CandidateReplied, ConversationStage.Closed),
        (ConversationStage.TopicStopped, ConversationStage.AwaitingCandidateReply),
        (ConversationStage.TopicStopped, ConversationStage.CandidateReplied),
        (ConversationStage.TopicStopped, ConversationStage.Qualified),
        (ConversationStage.TopicStopped, ConversationStage.Closed),
        (ConversationStage.Qualified, ConversationStage.Closed)
    ];

    private readonly ConversationStateMachine _stateMachine = new();

    public static IEnumerable<object[]> ValidTransitionCases()
    {
        return ValidTransitions.Select(transition => new object[] { transition.From, transition.To });
    }

    [Theory]
    [MemberData(nameof(ValidTransitionCases))]
    public void Allows_valid_state_transitions(ConversationStage from, ConversationStage to)
    {
        var conversation = CreateConversation(from);

        _stateMachine.TransitionTo(conversation, to);

        Assert.Equal(to, conversation.Stage);
    }

    [Fact]
    public void Rejects_all_undefined_state_transitions()
    {
        var validTransitions = ValidTransitions.ToHashSet();

        foreach (var from in Enum.GetValues<ConversationStage>())
        {
            foreach (var to in Enum.GetValues<ConversationStage>())
            {
                if (validTransitions.Contains((from, to)))
                {
                    continue;
                }

                var conversation = CreateConversation(from);

                Assert.False(_stateMachine.CanTransition(from, to));
                Assert.Throws<InvalidOperationException>(() => _stateMachine.TransitionTo(conversation, to));
            }
        }
    }

    [Fact]
    public void Clear_refusal_stops_that_topic()
    {
        var conversation = CreateConversation();

        _stateMachine.RecordMessage(conversation, AgentMessage("Do you like horror?", InterestType.Horror));
        _stateMachine.RecordMessage(conversation, CandidateMessage(
            "No. Please do not ask me about horror.",
            InterestType.Horror,
            ConsentState.Refused));

        Assert.True(conversation.IsTopicStopped(InterestType.Horror));
        Assert.False(_stateMachine.CanDiscussTopic(conversation, InterestType.Horror));
        Assert.False(_stateMachine.CanSendAutomaticMessage(conversation, InterestType.Horror));
    }

    [Fact]
    public void Clear_refusal_does_not_stop_other_topics()
    {
        var conversation = CreateConversation();

        _stateMachine.RecordMessage(conversation, AgentMessage("Do you like horror?", InterestType.Horror));
        _stateMachine.RecordMessage(conversation, CandidateMessage(
            "No. Please do not ask me about horror.",
            InterestType.Horror,
            ConsentState.Refused));

        Assert.True(_stateMachine.CanDiscussTopic(conversation, InterestType.MortalKombat));
        Assert.True(_stateMachine.CanSendAutomaticMessage(conversation, InterestType.MortalKombat));
    }

    [Fact]
    public void No_reply_means_no_additional_initiated_message()
    {
        var conversation = CreateConversation();

        _stateMachine.RecordMessage(conversation, AgentMessage("Hello."));

        Assert.Equal(ConversationStage.AwaitingCandidateReply, conversation.Stage);
        Assert.False(_stateMachine.CanSendAutomaticMessage(conversation));
        Assert.Throws<InvalidOperationException>(() => _stateMachine.RecordMessage(
            conversation,
            AgentMessage("Following up before the candidate replied.")));
    }

    [Fact]
    public void Candidate_reply_allows_next_automatic_message()
    {
        var conversation = CreateConversation();

        _stateMachine.RecordMessage(conversation, AgentMessage("Hello."));
        _stateMachine.RecordMessage(conversation, CandidateMessage("Hi."));

        Assert.Equal(ConversationStage.CandidateReplied, conversation.Stage);
        Assert.True(_stateMachine.CanSendAutomaticMessage(conversation));
    }

    [Fact]
    public void Qualified_candidates_stop_automatic_conversation_actions()
    {
        var conversation = CreateConversation(ConversationStage.CandidateReplied);

        _stateMachine.TransitionTo(conversation, ConversationStage.Qualified);

        Assert.Equal(CandidateStatus.Qualified, conversation.CandidateProfile.Status);
        Assert.False(_stateMachine.CanSendAutomaticMessage(conversation));
    }

    [Fact]
    public void Qualified_candidate_status_stops_automatic_actions_even_if_stage_is_not_qualified()
    {
        var candidate = new CandidateProfile("candidate-1", "Candidate");
        candidate.MarkQualified();
        var conversation = new Conversation("conversation-1", candidate, ConversationStage.CandidateReplied);

        Assert.False(_stateMachine.CanSendAutomaticMessage(conversation));
    }

    private static Conversation CreateConversation(ConversationStage stage = ConversationStage.NotStarted)
    {
        return new Conversation(
            "conversation-1",
            new CandidateProfile("candidate-1", "Candidate"),
            stage);
    }

    private static ConversationMessage AgentMessage(string text, InterestType? topic = null)
    {
        return new ConversationMessage(
            MessageAuthor.Agent,
            text,
            DateTimeOffset.UtcNow,
            topic);
    }

    private static ConversationMessage CandidateMessage(
        string text,
        InterestType? topic = null,
        ConsentState consentState = ConsentState.Unknown)
    {
        return new ConversationMessage(
            MessageAuthor.Candidate,
            text,
            DateTimeOffset.UtcNow,
            topic,
            consentState);
    }
}
