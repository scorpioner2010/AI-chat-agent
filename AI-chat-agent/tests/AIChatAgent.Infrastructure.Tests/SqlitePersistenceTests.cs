using AIChatAgent.Application.Abstractions;
using AIChatAgent.Domain.Entities;
using AIChatAgent.Domain.Enums;
using AIChatAgent.Domain.Services;
using AIChatAgent.Domain.ValueObjects;
using AIChatAgent.Infrastructure.Persistence;
using AIChatAgent.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AIChatAgent.Infrastructure.Tests;

public sealed class SqlitePersistenceTests
{
    [Fact]
    public async Task CandidateRepository_persists_candidate_status_and_interest_signals()
    {
        await using var database = await TemporarySqliteDatabase.CreateAsync();

        var candidate = new CandidateProfile("candidate-1", "Candidate One");
        candidate.MarkQualified();
        candidate.AddInterestSignal(new InterestSignal(
            InterestType.Horror,
            SignalValue.Positive,
            new InterestEvidence("Candidate explicitly likes horror.", 0.9m)));

        await using (var writeContext = database.CreateContext())
        {
            var repository = new CandidateRepository(writeContext);
            await repository.AddAsync(candidate, CancellationToken.None);
        }

        await using (var readContext = database.CreateContext())
        {
            var repository = new CandidateRepository(readContext);
            var loaded = await repository.GetByIdAsync(candidate.Id, CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal(candidate.Id, loaded.Id);
            Assert.Equal("Candidate One", loaded.DisplayName);
            Assert.Equal(CandidateStatus.Qualified, loaded.Status);
            var signal = Assert.Single(loaded.InterestSignals);
            Assert.Equal(InterestType.Horror, signal.Type);
            Assert.Equal(SignalValue.Positive, signal.Value);
            Assert.Equal("Candidate explicitly likes horror.", signal.Evidence.SourceText);
            Assert.Equal(0.9m, signal.Evidence.Confidence);
        }
    }

    [Fact]
    public async Task CandidateRepository_updates_existing_candidate_without_duplicate_signals()
    {
        await using var database = await TemporarySqliteDatabase.CreateAsync();

        var candidate = new CandidateProfile("candidate-1", "Candidate One");
        candidate.AddInterestSignal(new InterestSignal(
            InterestType.MortalKombat,
            SignalValue.Known,
            new InterestEvidence("Candidate recognized Mortal Kombat.")));

        await using (var writeContext = database.CreateContext())
        {
            var repository = new CandidateRepository(writeContext);
            await repository.AddAsync(candidate, CancellationToken.None);
            candidate.MarkQualified();
            await repository.UpdateAsync(candidate, CancellationToken.None);
            await repository.UpdateAsync(candidate, CancellationToken.None);
        }

        await using (var readContext = database.CreateContext())
        {
            var repository = new CandidateRepository(readContext);
            var loaded = await repository.GetByIdAsync(candidate.Id, CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal(CandidateStatus.Qualified, loaded.Status);
            Assert.Single(loaded.InterestSignals);
        }
    }

    [Fact]
    public async Task ConversationRepository_persists_conversation_state_messages_and_stopped_topics()
    {
        await using var database = await TemporarySqliteDatabase.CreateAsync();

        var conversation = CreateConversationWithRefusedTopic();

        await using (var writeContext = database.CreateContext())
        {
            var repository = new ConversationRepository(writeContext);
            await repository.AddAsync(conversation, CancellationToken.None);
        }

        await using (var readContext = database.CreateContext())
        {
            var repository = new ConversationRepository(readContext);
            var loaded = await repository.GetByIdAsync(conversation.Id, CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal(ConversationStage.TopicStopped, loaded.Stage);
            Assert.Equal(CandidateStatus.Active, loaded.CandidateProfile.Status);
            Assert.Equal(2, loaded.Messages.Count);
            Assert.Equal(MessageAuthor.Agent, loaded.Messages[0].Author);
            Assert.Equal(MessageAuthor.Candidate, loaded.Messages[1].Author);
            Assert.Equal(ConsentState.Refused, loaded.Messages[1].ConsentState);
            Assert.True(loaded.IsTopicStopped(InterestType.Horror));
        }
    }

    [Fact]
    public async Task ConversationRepository_gets_conversation_by_candidate_id()
    {
        await using var database = await TemporarySqliteDatabase.CreateAsync();

        var conversation = CreateConversationWithRefusedTopic();

        await using (var writeContext = database.CreateContext())
        {
            var repository = new ConversationRepository(writeContext);
            await repository.AddAsync(conversation, CancellationToken.None);
        }

        await using (var readContext = database.CreateContext())
        {
            var repository = new ConversationRepository(readContext);
            var loaded = await repository.GetByCandidateIdAsync(
                conversation.CandidateProfile.Id,
                CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal(conversation.Id, loaded.Id);
        }
    }

    [Fact]
    public async Task ConversationRepository_update_is_idempotent_for_messages()
    {
        await using var database = await TemporarySqliteDatabase.CreateAsync();

        var conversation = CreateConversationWithRefusedTopic();

        await using (var writeContext = database.CreateContext())
        {
            var repository = new ConversationRepository(writeContext);
            await repository.AddAsync(conversation, CancellationToken.None);
            await repository.UpdateAsync(conversation, CancellationToken.None);
            await repository.UpdateAsync(conversation, CancellationToken.None);
        }

        await using (var readContext = database.CreateContext())
        {
            var repository = new ConversationRepository(readContext);
            var loaded = await repository.GetByIdAsync(conversation.Id, CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal(2, loaded.Messages.Count);
        }
    }

    [Fact]
    public async Task ConversationRepository_prevents_duplicate_messages()
    {
        await using var database = await TemporarySqliteDatabase.CreateAsync();

        var candidate = new CandidateProfile("candidate-1", "Candidate One");
        var sentAt = new DateTimeOffset(2026, 7, 23, 12, 0, 0, TimeSpan.Zero);
        var duplicateMessage = new ConversationMessage(
            MessageAuthor.Agent,
            "Hello.",
            sentAt);
        var conversation = Conversation.Rehydrate(
            "conversation-1",
            candidate,
            ConversationStage.AwaitingCandidateReply,
            [duplicateMessage, duplicateMessage],
            []);

        await using var writeContext = database.CreateContext();
        var repository = new ConversationRepository(writeContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.AddAsync(conversation, CancellationToken.None));

        Assert.Contains("duplicate messages", exception.Message);
    }

    [Fact]
    public async Task Infrastructure_dependency_injection_registers_db_context_repositories_and_initializer()
    {
        await using var database = await TemporarySqliteDatabase.CreateAsync(initialize: false);
        var services = new ServiceCollection();
        services.AddAiChatAgentInfrastructure(database.ConnectionString);

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<AgentDatabaseInitializer>();
        await initializer.InitializeAsync(CancellationToken.None);

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<AgentDbContext>());
        Assert.IsType<CandidateRepository>(
            scope.ServiceProvider.GetRequiredService<ICandidateRepository>());
        Assert.IsType<ConversationRepository>(
            scope.ServiceProvider.GetRequiredService<IConversationRepository>());
    }

    private static Conversation CreateConversationWithRefusedTopic()
    {
        var candidate = new CandidateProfile("candidate-1", "Candidate One");
        candidate.AddInterestSignal(new InterestSignal(
            InterestType.Horror,
            SignalValue.Positive,
            new InterestEvidence("Candidate likes horror.")));

        var conversation = new Conversation("conversation-1", candidate);
        var stateMachine = new ConversationStateMachine();

        stateMachine.RecordMessage(conversation, new ConversationMessage(
            MessageAuthor.Agent,
            "Do you like horror?",
            new DateTimeOffset(2026, 7, 23, 12, 0, 0, TimeSpan.Zero),
            InterestType.Horror));

        stateMachine.RecordMessage(conversation, new ConversationMessage(
            MessageAuthor.Candidate,
            "No. Please do not ask me about horror.",
            new DateTimeOffset(2026, 7, 23, 12, 1, 0, TimeSpan.Zero),
            InterestType.Horror,
            ConsentState.Refused));

        return conversation;
    }

    private sealed class TemporarySqliteDatabase : IAsyncDisposable
    {
        private readonly string _directoryPath;

        private TemporarySqliteDatabase(string directoryPath)
        {
            _directoryPath = directoryPath;
            ConnectionString = $"Data Source={Path.Combine(directoryPath, "agent-tests.db")}";
        }

        public string ConnectionString { get; }

        public static async Task<TemporarySqliteDatabase> CreateAsync(bool initialize = true)
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "AIChatAgent.Infrastructure.Tests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directoryPath);

            var database = new TemporarySqliteDatabase(directoryPath);

            if (initialize)
            {
                await using var context = database.CreateContext();
                var initializer = new AgentDatabaseInitializer(context);
                await initializer.InitializeAsync(CancellationToken.None);
            }

            return database;
        }

        public AgentDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AgentDbContext>()
                .UseSqlite(ConnectionString)
                .Options;

            return new AgentDbContext(options);
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                if (Directory.Exists(_directoryPath))
                {
                    Directory.Delete(_directoryPath, recursive: true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return ValueTask.CompletedTask;
        }
    }
}
