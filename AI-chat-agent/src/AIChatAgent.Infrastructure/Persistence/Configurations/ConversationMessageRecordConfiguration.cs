using AIChatAgent.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIChatAgent.Infrastructure.Persistence.Configurations;

internal sealed class ConversationMessageRecordConfiguration : IEntityTypeConfiguration<ConversationMessageRecord>
{
    public void Configure(EntityTypeBuilder<ConversationMessageRecord> builder)
    {
        builder.ToTable("ConversationMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.ConversationId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(message => message.Ordinal)
            .IsRequired();

        builder.Property(message => message.Author)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(message => message.Text)
            .HasMaxLength(8192)
            .IsRequired();

        builder.Property(message => message.SentAt)
            .IsRequired();

        builder.Property(message => message.Topic)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(message => message.ConsentState)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(message => message.Fingerprint)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(message => message.ConversationId);
        builder.HasIndex(message => new { message.ConversationId, message.Ordinal })
            .IsUnique();
        builder.HasIndex(message => new { message.ConversationId, message.Fingerprint })
            .IsUnique();
    }
}
