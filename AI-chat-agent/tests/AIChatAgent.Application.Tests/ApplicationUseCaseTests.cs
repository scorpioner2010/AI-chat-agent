using AIChatAgent.Application.Abstractions;
using AIChatAgent.Application.UseCases;
using AIChatAgent.Domain.Entities;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.Services;
using AIChatAgent.Domain.ValueObjects;

namespace AIChatAgent.Application.Tests;

public sealed class ApplicationUseCaseTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 7, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task RegisterCandidate_registers_new_candidate_and_returns_summary()
    {
        var candidateRepository = new InMemoryCandidateRepository();
        var useCase = new RegisterCandidate(candidateRepository);

        var summary = await useCase.ExecuteAsync("candidate-1", "Candidate One", CancellationToken.None);

        Assert.Equal("candidate-1", summary.Id);
        Assert.Equal("Candidate One", summary.DisplayName);
        Assert.Equal(CandidateStatus.New, summary.Status);
        Assert.NotNull(await candidateRepository.GetByIdAsync("candidate-1", CancellationToken.None));
        Assert.Equal(1, candidateRepository.AddCount);
    }

    [Fact]
    public async Task RegisterCandidate_rejects_duplicate_candidate()
    {
        var candidateRepository = new InMemoryCandidateRepository();
        candidateRepository.Seed(new CandidateProfile("candidate-1", "Existing"));
        var useCase = new RegisterCandidate(candidateRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync("candidate-1", "Duplicate", CancellationToken.None));

        Assert.Contains("already registered", exception.Message);
    }

    [Fact]
    public async Task RegisterCandidate_validates_required_input()
    {
        var useCase = new RegisterCandidate(new InMemoryCandidateRepository());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(" ", "Candidate", CancellationToken.None));

        Assert.Contains("Candidate id is required", exception.Message);
    }

    [Fact]
    public async Task AddConversationMessage_records_message_with_timestamp_and_updates_stage()
    {
        var conversationRepository = new InMemoryConversationRepository();
        var conversation = CreateConversation();
        conversationRepository.Seed(conversation);
        var useCase = CreateAddConversationMessage(conversationRepository);

        var snapshot = await useCase.ExecuteAsync(
            conversation.Id,
            MessageAuthor.Agent,
            "Hello.",
            CancellationToken.None);

        Assert.Equal(ConversationStage.AwaitingCandidateReply, snapshot.Stage);
        Assert.Single(snapshot.Messages);
        Assert.Equal(FixedNow, snapshot.Messages[0].SentAt);
        Assert.Equal("Hello.", snapshot.Messages[0].Text);
        Assert.Equal(1, conversationRepository.UpdateCount);
    }

    [Fact]
    public async Task AddConversationMessage_prevents_second_initiated_message_before_candidate_reply()
    {
        var conversationRepository = new InMemoryConversationRepository();
        var conversation = CreateConversation();
        conversationRepository.Seed(conversation);
        var useCase = CreateAddConversationMessage(conversationRepository);

        await useCase.ExecuteAsync(
            conversation.Id,
            MessageAuthor.Agent,
            "Hello.",
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                conversation.Id,
                MessageAuthor.Agent,
                "Following up before reply.",
                CancellationToken.None));

        Assert.Contains("until the candidate replies", exception.Message);
    }

    [Fact]
    public async Task AddConversationMessage_clear_refusal_stops_that_topic()
    {
        var conversationRepository = new InMemoryConversationRepository();
        var conversation = CreateConversation();
        conversationRepository.Seed(conversation);
        var useCase = CreateAddConversationMessage(conversationRepository);

        await useCase.ExecuteAsync(
            conversation.Id,
            MessageAuthor.Agent,
            "Do you like horror?",
            CancellationToken.None,
            InterestType.Horror);

        var snapshot = await useCase.ExecuteAsync(
            conversation.Id,
            MessageAuthor.Candidate,
            "No. Do not ask me about horror.",
            CancellationToken.None,
            InterestType.Horror,
            ConsentState.Refused);

        Assert.Equal(ConversationStage.TopicStopped, snapshot.Stage);
        Assert.Contains(InterestType.Horror, snapshot.StoppedTopics);
    }

    [Fact]
    public async Task AddConversationMessage_throws_clear_error_when_conversation_is_missing()
    {
        var useCase = CreateAddConversationMessage(new InMemoryConversationRepository());

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(
                "missing-conversation",
                MessageAuthor.Agent,
                "Hello.",
                CancellationToken.None));

        Assert.Contains("missing-conversation", exception.Message);
    }

    [Fact]
    public async Task GetConversation_returns_snapshot()
    {
        var conversationRepository = new InMemoryConversationRepository();
        var conversation = CreateConversation(ConversationStage.CandidateReplied);
        conversationRepository.Seed(conversation);
        var useCase = new GetConversation(conversationRepository);

        var snapshot = await useCase.ExecuteAsync(conversation.Id, CancellationToken.None);

        Assert.Equal(conversation.Id, snapshot.Id);
        Assert.Equal(ConversationStage.CandidateReplied, snapshot.Stage);
        Assert.Equal(conversation.CandidateProfile.Id, snapshot.Candidate.Id);
    }

    [Fact]
    public async Task GetConversation_throws_clear_error_when_missing()
    {
        var useCase = new GetConversation(new InMemoryConversationRepository());

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync("missing-conversation", CancellationToken.None));

        Assert.Contains("missing-conversation", exception.Message);
    }

    [Fact]
    public async Task EvaluateCandidate_scores_candidate_with_domain_scorer()
    {
        var candidateRepository = new InMemoryCandidateRepository();
        var candidate = new CandidateProfile("candidate-1", "Candidate");
        candidate.AddInterestSignal(new InterestSignal(
            InterestType.Horror,
            SignalValue.Positive,
            new InterestEvidence("Candidate said they enjoy horror.")));
        candidateRepository.Seed(candidate);
        var useCase = new EvaluateCandidate(candidateRepository, new CompatibilityScorer());

        var result = await useCase.ExecuteAsync(
            candidate.Id,
            [InterestType.Horror],
            CancellationToken.None,
            shortlistThreshold: 60);

        Assert.Equal(70, result.CompatibilityScore.Value);
        Assert.True(result.ShouldShortlist);
        Assert.Equal(1, result.Candidate.InterestSignalCount);
    }

    [Fact]
    public async Task EvaluateCandidate_validates_score_threshold()
    {
        var candidateRepository = new InMemoryCandidateRepository();
        candidateRepository.Seed(new CandidateProfile("candidate-1", "Candidate"));
        var useCase = new EvaluateCandidate(candidateRepository, new CompatibilityScorer());

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            useCase.ExecuteAsync(
                "candidate-1",
                [InterestType.Horror],
                CancellationToken.None,
                shortlistThreshold: -1));

        Assert.Contains("Compatibility score must be between", exception.Message);
    }

    [Fact]
    public async Task MarkCandidateAsQualified_uses_state_machine_and_updates_repositories()
    {
        var candidateRepository = new InMemoryCandidateRepository();
        var conversationRepository = new InMemoryConversationRepository();
        var conversation = CreateConversation(ConversationStage.CandidateReplied);
        candidateRepository.Seed(conversation.CandidateProfile);
        conversationRepository.Seed(conversation);
        var useCase = new MarkCandidateAsQualified(
            candidateRepository,
            conversationRepository,
            new ConversationStateMachine());

        var summary = await useCase.ExecuteAsync(conversation.Id, CancellationToken.None);

        Assert.Equal(CandidateStatus.Qualified, summary.Status);
        Assert.Equal(ConversationStage.Qualified, conversation.Stage);
        Assert.Equal(1, candidateRepository.UpdateCount);
        Assert.Equal(1, conversationRepository.UpdateCount);
    }

    [Fact]
    public async Task MarkCandidateAsQualified_rejects_invalid_transition()
    {
        var candidateRepository = new InMemoryCandidateRepository();
        var conversationRepository = new InMemoryConversationRepository();
        var conversation = CreateConversation(ConversationStage.NotStarted);
        candidateRepository.Seed(conversation.CandidateProfile);
        conversationRepository.Seed(conversation);
        var useCase = new MarkCandidateAsQualified(
            candidateRepository,
            conversationRepository,
            new ConversationStateMachine());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(conversation.Id, CancellationToken.None));

        Assert.Contains("Cannot transition", exception.Message);
    }

    [Fact]
    public async Task StopConversation_closes_conversation()
    {
        var conversationRepository = new InMemoryConversationRepository();
        var conversation = CreateConversation(ConversationStage.CandidateReplied);
        conversationRepository.Seed(conversation);
        var useCase = new StopConversation(conversationRepository, new ConversationStateMachine());

        var snapshot = await useCase.ExecuteAsync(conversation.Id, CancellationToken.None);

        Assert.Equal(ConversationStage.Closed, snapshot.Stage);
        Assert.Equal(1, conversationRepository.UpdateCount);
    }

    [Fact]
    public async Task StopConversation_throws_clear_error_when_missing()
    {
        var useCase = new StopConversation(
            new InMemoryConversationRepository(),
            new ConversationStateMachine());

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync("missing-conversation", CancellationToken.None));

        Assert.Contains("missing-conversation", exception.Message);
    }

    private static AddConversationMessage CreateAddConversationMessage(
        InMemoryConversationRepository conversationRepository)
    {
        return new AddConversationMessage(
            conversationRepository,
            new ConversationStateMachine(),
            new FixedDateTimeProvider(FixedNow));
    }

    private static Conversation CreateConversation(
        ConversationStage stage = ConversationStage.NotStarted)
    {
        return new Conversation(
            "conversation-1",
            new CandidateProfile("candidate-1", "Candidate"),
            stage);
    }

    private sealed class InMemoryCandidateRepository : ICandidateRepository
    {
        private readonly Dictionary<string, CandidateProfile> _candidates = new();

        public int AddCount { get; private set; }

        public int UpdateCount { get; private set; }

        public void Seed(CandidateProfile candidateProfile)
        {
            _candidates[candidateProfile.Id] = candidateProfile;
        }

        public Task<CandidateProfile?> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _candidates.TryGetValue(id, out var candidateProfile);
            return Task.FromResult(candidateProfile);
        }

        public Task AddAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_candidates.TryAdd(candidateProfile.Id, candidateProfile))
            {
                throw new InvalidOperationException($"Candidate '{candidateProfile.Id}' already exists.");
            }

            AddCount++;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _candidates[candidateProfile.Id] = candidateProfile;
            UpdateCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryConversationRepository : IConversationRepository
    {
        private readonly Dictionary<string, Conversation> _conversations = new();

        public int AddCount { get; private set; }

        public int UpdateCount { get; private set; }

        public void Seed(Conversation conversation)
        {
            _conversations[conversation.Id] = conversation;
        }

        public Task<Conversation?> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _conversations.TryGetValue(id, out var conversation);
            return Task.FromResult(conversation);
        }

        public Task<Conversation?> GetByCandidateIdAsync(
            string candidateId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var conversation = _conversations.Values
                .FirstOrDefault(item => item.CandidateProfile.Id == candidateId);

            return Task.FromResult(conversation);
        }

        public Task AddAsync(Conversation conversation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_conversations.TryAdd(conversation.Id, conversation))
            {
                throw new InvalidOperationException($"Conversation '{conversation.Id}' already exists.");
            }

            AddCount++;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _conversations[conversation.Id] = conversation;
            UpdateCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public FixedDateTimeProvider(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
