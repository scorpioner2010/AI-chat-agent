using AIChatAgent.Application.DTOs;

namespace AIChatAgent.Application.Abstractions;

public interface IPlatformAdapter
{
    string PlatformName { get; }

    Task<CandidateSummary?> GetCandidateAsync(string platformCandidateId, CancellationToken cancellationToken);

    Task<ConversationSnapshot?> GetConversationAsync(string platformConversationId, CancellationToken cancellationToken);

    Task SendReplyAsync(
        string platformConversationId,
        ReplySuggestion replySuggestion,
        CancellationToken cancellationToken);
}
