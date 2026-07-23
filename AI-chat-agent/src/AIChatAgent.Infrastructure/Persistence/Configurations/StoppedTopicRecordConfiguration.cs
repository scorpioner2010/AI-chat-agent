using AIChatAgent.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIChatAgent.Infrastructure.Persistence.Configurations;

internal sealed class StoppedTopicRecordConfiguration : IEntityTypeConfiguration<StoppedTopicRecord>
{
    public void Configure(EntityTypeBuilder<StoppedTopicRecord> builder)
    {
        builder.ToTable("StoppedTopics");

        builder.HasKey(topic => new { topic.ConversationId, topic.Topic });

        builder.Property(topic => topic.ConversationId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(topic => topic.Topic)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(topic => topic.Topic);
    }
}
