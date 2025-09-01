using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeIdToPosCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "PosCharges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PosCharges_EmployeeId",
                table: "PosCharges",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PosCharges_Employees_EmployeeId",
                table: "PosCharges",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PosCharges_Employees_EmployeeId",
                table: "PosCharges");

            migrationBuilder.DropIndex(
                name: "IX_PosCharges_EmployeeId",
                table: "PosCharges");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "PosCharges");
        }
    }
}
