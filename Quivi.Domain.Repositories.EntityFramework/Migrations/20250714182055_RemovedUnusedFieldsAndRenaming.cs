using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnusedFieldsAndRenaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Journals");

            migrationBuilder.RenameColumn(
                name: "JournalId",
                table: "Journals",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "JournalHistoryId",
                table: "JournalChanges",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "ChargePartner",
                table: "Charges",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ChargeMethod",
                table: "Charges",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Journals",
                newName: "JournalId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "JournalChanges",
                newName: "JournalHistoryId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Journals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Journals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Journals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ChargePartner",
                table: "Charges",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ChargeMethod",
                table: "Charges",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
