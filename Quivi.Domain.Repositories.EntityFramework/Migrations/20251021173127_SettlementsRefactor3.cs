using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SettlementsRefactor3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementServiceDetails",
                table: "SettlementServiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_SettlementServiceDetails_JournalId",
                table: "SettlementServiceDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementDetails",
                table: "SettlementDetails");

            migrationBuilder.DropIndex(
                name: "IX_SettlementDetails_JournalId",
                table: "SettlementDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementServiceDetails",
                table: "SettlementServiceDetails",
                column: "JournalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementDetails",
                table: "SettlementDetails",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementServiceDetails_SettlementId",
                table: "SettlementServiceDetails",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_Date",
                table: "Settlements",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDetails_SettlementId",
                table: "SettlementDetails",
                column: "SettlementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementServiceDetails",
                table: "SettlementServiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_SettlementServiceDetails_SettlementId",
                table: "SettlementServiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_Settlements_Date",
                table: "Settlements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementDetails",
                table: "SettlementDetails");

            migrationBuilder.DropIndex(
                name: "IX_SettlementDetails_SettlementId",
                table: "SettlementDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementServiceDetails",
                table: "SettlementServiceDetails",
                columns: new[] { "SettlementId", "JournalId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementDetails",
                table: "SettlementDetails",
                columns: new[] { "SettlementId", "JournalId" });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementServiceDetails_JournalId",
                table: "SettlementServiceDetails",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDetails_JournalId",
                table: "SettlementDetails",
                column: "JournalId");
        }
    }
}
