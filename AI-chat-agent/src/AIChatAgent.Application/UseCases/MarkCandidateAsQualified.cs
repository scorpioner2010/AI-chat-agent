using AIChatAgent.Application.Abstractions;
using AIChatAgent.Application.DTOs;
using AIChatAgent.Application.Mapping;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.Services;

namespace AIChatAgent.Application.UseCases;

public sealed class MarkCandidateAsQualified
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ConversationStateMachine _conversationStateMachine;

    public MarkCandidateAsQualified(
        ICandidateRepository candidateRepository,
        IConversationRepository conversationRepository,
        ConversationStateMachine conversationStateMachine)
    {
        _candidateRepository = candidateRepository
            ?? throw new ArgumentNullException(nameof(candidateRepository));
        _conversationRepository = conversationRepository
            ?? throw new ArgumentNullException(nameof(conversationRepository));
        _conversationStateMachine = conversationStateMachine
            ?? throw new ArgumentNullException(nameof(conversationStateMachine));
    }

    public async Task<CandidateSummary> ExecuteAsync(
        string conversationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(conversationId));
        }

        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation '{conversationId}' was not found.");

        _conversationStateMachine.TransitionTo(conversation, ConversationStage.Qualified);

        await _candidateRepository.UpdateAsync(conversation.CandidateProfile, cancellationToken);
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return ApplicationMapper.ToCandidateSummary(conversation.CandidateProfile);
    }
}
