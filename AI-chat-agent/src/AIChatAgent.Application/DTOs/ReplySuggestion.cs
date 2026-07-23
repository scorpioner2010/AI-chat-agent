namespace AIChatAgent.Application.DTOs;

public sealed record ReplySuggestion
{
    public ReplySuggestion(
        string conversationId,
        string text,
        bool requiresUserReview = true)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Reply suggestion conversation id is required.", nameof(conversationId));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Reply suggestion text is required.", nameof(text));
        }

        ConversationId = conversationId;
        Text = text;
        RequiresUserReview = requiresUserReview;
    }

    public string ConversationId { get; }

    public string Text { get; }

    public bool RequiresUserReview { get; }
}
