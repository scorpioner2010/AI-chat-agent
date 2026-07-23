using AIChatAgent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIChatAgent.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AgentDbContext))]
[Migration("20260723000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Candidates",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Candidates", candidate => candidate.Id);
            });

        migrationBuilder.CreateTable(
            name: "Conversations",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                CandidateId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Stage = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Conversations", conversation => conversation.Id);
                table.ForeignKey(
                    name: "FK_Conversations_Candidates_CandidateId",
                    column: conversation => conversation.CandidateId,
                    principalTable: "Candidates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "InterestSignals",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CandidateId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Type = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Value = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                EvidenceSourceText = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                EvidenceConfidence = table.Column<double>(type: "REAL", nullable: false),
                EvidenceConsentState = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Fingerprint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InterestSignals", signal => signal.Id);
                table.ForeignKey(
                    name: "FK_InterestSignals_Candidates_CandidateId",
                    column: signal => signal.CandidateId,
                    principalTable: "Candidates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ConversationMessages",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ConversationId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                Author = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Text = table.Column<string>(type: "TEXT", maxLength: 8192, nullable: false),
                SentAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                Topic = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                ConsentState = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Fingerprint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConversationMessages", message => message.Id);
                table.ForeignKey(
                    name: "FK_ConversationMessages_Conversations_ConversationId",
                    column: message => message.ConversationId,
                    principalTable: "Conversations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StoppedTopics",
            columns: table => new
            {
                ConversationId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Topic = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StoppedTopics", topic => new { topic.ConversationId, topic.Topic });
                table.ForeignKey(
                    name: "FK_StoppedTopics_Conversations_ConversationId",
                    column: topic => topic.ConversationId,
                    principalTable: "Conversations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Candidates_DisplayName",
            table: "Candidates",
            column: "DisplayName");

        migrationBuilder.CreateIndex(
            name: "IX_Candidates_Status",
            table: "Candidates",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_ConversationMessages_ConversationId",
            table: "ConversationMessages",
            column: "ConversationId");

        migrationBuilder.CreateIndex(
            name: "IX_ConversationMessages_ConversationId_Fingerprint",
            table: "ConversationMessages",
            columns: new[] { "ConversationId", "Fingerprint" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ConversationMessages_ConversationId_Ordinal",
            table: "ConversationMessages",
            columns: new[] { "ConversationId", "Ordinal" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Conversations_CandidateId",
            table: "Conversations",
            column: "CandidateId");

        migrationBuilder.CreateIndex(
            name: "IX_Conversations_Stage",
            table: "Conversations",
            column: "Stage");

        migrationBuilder.CreateIndex(
            name: "IX_InterestSignals_CandidateId",
            table: "InterestSignals",
            column: "CandidateId");

        migrationBuilder.CreateIndex(
            name: "IX_InterestSignals_CandidateId_Fingerprint",
            table: "InterestSignals",
            columns: new[] { "CandidateId", "Fingerprint" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StoppedTopics_Topic",
            table: "StoppedTopics",
            column: "Topic");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ConversationMessages");
        migrationBuilder.DropTable(name: "InterestSignals");
        migrationBuilder.DropTable(name: "StoppedTopics");
        migrationBuilder.DropTable(name: "Conversations");
        migrationBuilder.DropTable(name: "Candidates");
    }
}
