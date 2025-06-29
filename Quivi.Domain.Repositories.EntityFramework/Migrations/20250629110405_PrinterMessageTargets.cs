using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class PrinterMessageTargets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrinterNotificationMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageType = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterNotificationMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrinterNotificationMessage_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrinterMessageTarget",
                columns: table => new
                {
                    PrinterNotificationMessageId = table.Column<int>(type: "int", nullable: false),
                    PrinterNotificationsContactId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterMessageTarget", x => new { x.PrinterNotificationsContactId, x.PrinterNotificationMessageId });
                    table.ForeignKey(
                        name: "FK_PrinterMessageTarget_PrinterNotificationMessage_PrinterNotificationMessageId",
                        column: x => x.PrinterNotificationMessageId,
                        principalTable: "PrinterNotificationMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrinterMessageTarget_PrinterNotificationsContact_PrinterNotificationsContactId",
                        column: x => x.PrinterNotificationsContactId,
                        principalTable: "PrinterNotificationsContact",
                        principalColumn: "NotificationsContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrinterMessageTarget_PrinterNotificationMessageId",
                table: "PrinterMessageTarget",
                column: "PrinterNotificationMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterNotificationMessage_MerchantId",
                table: "PrinterNotificationMessage",
                column: "MerchantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrinterMessageTarget");

            migrationBuilder.DropTable(
                name: "PrinterNotificationMessage");
        }
    }
}
