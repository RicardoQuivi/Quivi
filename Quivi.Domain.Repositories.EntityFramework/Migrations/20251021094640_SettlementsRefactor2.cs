using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SettlementsRefactor2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_Merchants_SubMerchantId",
                table: "People");

            migrationBuilder.RenameColumn(
                name: "SubMerchantId",
                table: "People",
                newName: "MerchantId");

            migrationBuilder.RenameIndex(
                name: "IX_People_SubMerchantId",
                table: "People",
                newName: "IX_People_MerchantId");

            migrationBuilder.AddForeignKey(
                name: "FK_People_Merchants_MerchantId",
                table: "People",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_Merchants_MerchantId",
                table: "People");

            migrationBuilder.RenameColumn(
                name: "MerchantId",
                table: "People",
                newName: "SubMerchantId");

            migrationBuilder.RenameIndex(
                name: "IX_People_MerchantId",
                table: "People",
                newName: "IX_People_SubMerchantId");

            migrationBuilder.AddForeignKey(
                name: "FK_People_Merchants_SubMerchantId",
                table: "People",
                column: "SubMerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");
        }
    }
}
