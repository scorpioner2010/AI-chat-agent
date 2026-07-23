using AIChatAgent.Domain.Enums;

namespace AIChatAgent.Infrastructure.Persistence.Entities;

internal sealed class InterestSignalRecord
{
    public int Id { get; set; }

    public string CandidateId { get; set; } = string.Empty;

    public InterestType Type { get; set; }

    public SignalValue Value { get; set; }

    public string EvidenceSourceText { get; set; } = string.Empty;

    public decimal EvidenceConfidence { get; set; }

    public ConsentState EvidenceConsentState { get; set; }

    public string Fingerprint { get; set; } = string.Empty;

    public CandidateRecord? Candidate { get; set; }
}
