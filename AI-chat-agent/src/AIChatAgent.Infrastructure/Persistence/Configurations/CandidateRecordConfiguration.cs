using AIChatAgent.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIChatAgent.Infrastructure.Persistence.Configurations;

internal sealed class CandidateRecordConfiguration : IEntityTypeConfiguration<CandidateRecord>
{
    public void Configure(EntityTypeBuilder<CandidateRecord> builder)
    {
        builder.ToTable("Candidates");

        builder.HasKey(candidate => candidate.Id);

        builder.Property(candidate => candidate.Id)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(candidate => candidate.DisplayName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(candidate => candidate.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(candidate => candidate.DisplayName);
        builder.HasIndex(candidate => candidate.Status);

        builder.HasMany(candidate => candidate.InterestSignals)
            .WithOne(signal => signal.Candidate)
            .HasForeignKey(signal => signal.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(candidate => candidate.Conversations)
            .WithOne(conversation => conversation.Candidate)
            .HasForeignKey(conversation => conversation.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
