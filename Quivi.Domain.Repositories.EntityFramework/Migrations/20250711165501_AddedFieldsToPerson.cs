using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedFieldsToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_People_SessionGuid",
                table: "People");

            migrationBuilder.DropColumn(
                name: "SessionGuid",
                table: "People");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "People",
                newName: "Id");

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "People",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "People",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_IsAnonymous",
                table: "People",
                column: "IsAnonymous",
                unique: true,
                filter: "[IsAnonymous] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_People_UserId",
                table: "People",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_People_AspNetUsers_UserId",
                table: "People",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_AspNetUsers_UserId",
                table: "People");

            migrationBuilder.DropIndex(
                name: "IX_People_IsAnonymous",
                table: "People");

            migrationBuilder.DropIndex(
                name: "IX_People_UserId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "People");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "People");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "People",
                newName: "PersonId");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionGuid",
                table: "People",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_SessionGuid",
                table: "People",
                column: "SessionGuid");
        }
    }
}
