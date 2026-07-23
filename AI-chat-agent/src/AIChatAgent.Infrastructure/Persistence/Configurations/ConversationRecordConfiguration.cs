using AIChatAgent.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIChatAgent.Infrastructure.Persistence.Configurations;

internal sealed class ConversationRecordConfiguration : IEntityTypeConfiguration<ConversationRecord>
{
    public void Configure(EntityTypeBuilder<ConversationRecord> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(conversation => conversation.Id);

        builder.Property(conversation => conversation.Id)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(conversation => conversation.CandidateId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(conversation => conversation.Stage)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(conversation => conversation.CandidateId);
        builder.HasIndex(conversation => conversation.Stage);

        builder.HasMany(conversation => conversation.Messages)
            .WithOne(message => message.Conversation)
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(conversation => conversation.StoppedTopics)
            .WithOne(topic => topic.Conversation)
            .HasForeignKey(topic => topic.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
