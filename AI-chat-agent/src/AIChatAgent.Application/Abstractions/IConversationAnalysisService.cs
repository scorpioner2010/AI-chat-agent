using AIChatAgent.Application.DTOs;
using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Application.Abstractions;

public interface IConversationAnalysisService
{
    Task<AnalysisResult> AnalyzeAsync(
        ConversationSnapshot conversation,
        IReadOnlyCollection<InterestType> desiredInterests,
        CancellationToken cancellationToken);

    Task<ReplySuggestion> SuggestReplyAsync(
        ConversationSnapshot conversation,
        AnalysisResult analysisResult,
        CancellationToken cancellationToken);
}
