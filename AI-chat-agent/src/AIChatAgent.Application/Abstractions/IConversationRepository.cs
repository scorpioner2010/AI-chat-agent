using AIChatAgent.Domain.Entities;

namespace AIChatAgent.Application.Abstractions;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<Conversation?> GetByCandidateIdAsync(string candidateId, CancellationToken cancellationToken);

    Task AddAsync(Conversation conversation, CancellationToken cancellationToken);

    Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken);
}
