using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnOnChangeLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EatOrderState",
                table: "OrderChangeLogs",
                newName: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                table: "OrderChangeLogs",
                newName: "EatOrderState");
        }
    }
}