using AIChatAgent.Application.Abstractions;
using AIChatAgent.Application.DTOs;
using AIChatAgent.Application.Mapping;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.Services;

namespace AIChatAgent.Application.UseCases;

public sealed class StopConversation
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ConversationStateMachine _conversationStateMachine;

    public StopConversation(
        IConversationRepository conversationRepository,
        ConversationStateMachine conversationStateMachine)
    {
        _conversationRepository = conversationRepository
            ?? throw new ArgumentNullException(nameof(conversationRepository));
        _conversationStateMachine = conversationStateMachine
            ?? throw new ArgumentNullException(nameof(conversationStateMachine));
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

        _conversationStateMachine.TransitionTo(conversation, ConversationStage.Closed);

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return ApplicationMapper.ToConversationSnapshot(conversation);
    }
}
