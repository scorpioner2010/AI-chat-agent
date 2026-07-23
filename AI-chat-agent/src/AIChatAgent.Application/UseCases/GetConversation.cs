using AIChatAgent.Application.Abstractions;
using AIChatAgent.Application.DTOs;
using AIChatAgent.Application.Mapping;

namespace AIChatAgent.Application.UseCases;

public sealed class GetConversation
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversation(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository
            ?? throw new ArgumentNullException(nameof(conversationRepository));
    }

    public async Task<ConversationSnapshot> ExecuteAsync(
        string conversationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(conversationId));
        }

        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation '{conversationId}' was not found.");

        return ApplicationMapper.ToConversationSnapshot(conversation);
    }
}
