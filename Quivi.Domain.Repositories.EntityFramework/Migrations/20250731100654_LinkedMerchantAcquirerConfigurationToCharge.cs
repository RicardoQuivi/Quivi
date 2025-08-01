using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class LinkedMerchantAcquirerConfigurationToCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MerchantAcquirerConfigurationId",
                table: "Charges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Charges_MerchantAcquirerConfigurationId",
                table: "Charges",
                column: "MerchantAcquirerConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Charges_MerchantAcquirerConfiguration_MerchantAcquirerConfigurationId",
                table: "Charges",
                column: "MerchantAcquirerConfigurationId",
                principalTable: "MerchantAcquirerConfiguration",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Charges_MerchantAcquirerConfiguration_MerchantAcquirerConfigurationId",
                table: "Charges");

            migrationBuilder.DropIndex(
                name: "IX_Charges_MerchantAcquirerConfigurationId",
                table: "Charges");

            migrationBuilder.DropColumn(
                name: "MerchantAcquirerConfigurationId",
                table: "Charges");
        }
    }
}