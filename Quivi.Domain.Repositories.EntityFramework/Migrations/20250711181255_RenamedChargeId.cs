using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RenamedChargeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChargeId",
                table: "Charges",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Charges",
                newName: "ChargeId");
        }
    }
}