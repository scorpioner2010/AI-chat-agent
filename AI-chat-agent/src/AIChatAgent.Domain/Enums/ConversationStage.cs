namespace AIChatAgent.Domain.Enums;

public enum ConversationStage
{
    NotStarted = 0,
    AwaitingCandidateReply = 1,
    CandidateReplied = 2,
    TopicStopped = 3,
    Qualified = 4,
    Closed = 5
}
