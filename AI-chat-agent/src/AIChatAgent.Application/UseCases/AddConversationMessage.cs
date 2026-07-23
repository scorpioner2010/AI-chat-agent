using AIChatAgent.Application.Abstractions;
using AIChatAgent.Application.DTOs;
using AIChatAgent.Application.Mapping;
using AIChatAgent.Domain.Entities;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.Services;

namespace AIChatAgent.Application.UseCases;

public sealed class AddConversationMessage
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ConversationStateMachine _conversationStateMachine;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddConversationMessage(
        IConversationRepository conversationRepository,
        ConversationStateMachine conversationStateMachine,
        IDateTimeProvider dateTimeProvider)
    {
        _conversationRepository = conversationRepository
            ?? throw new ArgumentNullException(nameof(conversationRepository));
        _conversationStateMachine = conversationStateMachine
            ?? throw new ArgumentNullException(nameof(conversationStateMachine));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<ConversationSnapshot> ExecuteAsync(
        string conversationId,
        MessageAuthor author,
        string text,
        CancellationToken cancellationToken,
        InterestType? topic = null,
        ConsentState consentState = ConsentState.Unknown)
    {
        EnsureRequired(conversationId, nameof(conversationId), "Conversation id is required.");
        EnsureRequired(text, nameof(text), "Conversation message text is required.");

        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation '{conversationId}' was not found.");

        var message = new ConversationMessage(
            author,
            text,
            _dateTimeProvider.UtcNow,
            topic,
            consentState);

        _conversationStateMachine.RecordMessage(conversation, message);

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return ApplicationMapper.ToConversationSnapshot(conversation);
    }

    private static void EnsureRequired(string value, string parameterName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
