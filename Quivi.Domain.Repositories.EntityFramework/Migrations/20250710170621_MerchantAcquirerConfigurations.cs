using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class MerchantAcquirerConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MerchantAcquirerConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChargePartner = table.Column<int>(type: "int", nullable: false),
                    ChargeMethod = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TerminalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebhookSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternallySettled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantAcquirerConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantAcquirerConfiguration_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantAcquirerConfiguration_ChargeMethod",
                table: "MerchantAcquirerConfiguration",
                column: "ChargeMethod");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantAcquirerConfiguration_ChargePartner",
                table: "MerchantAcquirerConfiguration",
                column: "ChargePartner");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantAcquirerConfiguration_MerchantId",
                table: "MerchantAcquirerConfiguration",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "MerchantAcquirerConfiguration",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantAcquirerConfiguration");
        }
    }
}
