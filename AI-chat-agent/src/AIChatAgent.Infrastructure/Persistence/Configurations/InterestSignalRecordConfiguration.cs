using AIChatAgent.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIChatAgent.Infrastructure.Persistence.Configurations;

internal sealed class InterestSignalRecordConfiguration : IEntityTypeConfiguration<InterestSignalRecord>
{
    public void Configure(EntityTypeBuilder<InterestSignalRecord> builder)
    {
        builder.ToTable("InterestSignals");

        builder.HasKey(signal => signal.Id);

        builder.Property(signal => signal.CandidateId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(signal => signal.Type)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(signal => signal.Value)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(signal => signal.EvidenceSourceText)
            .HasMaxLength(4096)
            .IsRequired();

        builder.Property(signal => signal.EvidenceConfidence)
            .HasConversion<double>()
            .IsRequired();

        builder.Property(signal => signal.EvidenceConsentState)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(signal => signal.Fingerprint)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(signal => signal.CandidateId);
        builder.HasIndex(signal => new { signal.CandidateId, signal.Fingerprint })
            .IsUnique();
    }
}
