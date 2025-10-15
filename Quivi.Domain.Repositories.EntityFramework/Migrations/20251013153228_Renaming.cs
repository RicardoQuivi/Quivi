using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Renaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PosChargeInvoiceItem_OrderMenuItems_OrderMenuItemId",
                table: "PosChargeInvoiceItem");

            migrationBuilder.DropForeignKey(
                name: "FK_PosChargeInvoiceItem_PosChargeInvoiceItem_ParentPosChargeInvoiceItemId",
                table: "PosChargeInvoiceItem");

            migrationBuilder.DropForeignKey(
                name: "FK_PosChargeInvoiceItem_PosCharges_PosChargeId",
                table: "PosChargeInvoiceItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PosChargeInvoiceItem",
                table: "PosChargeInvoiceItem");

            migrationBuilder.RenameTable(
                name: "PosChargeInvoiceItem",
                newName: "PosChargeInvoiceItems");

            migrationBuilder.RenameIndex(
                name: "IX_PosChargeInvoiceItem_PosChargeId",
                table: "PosChargeInvoiceItems",
                newName: "IX_PosChargeInvoiceItems_PosChargeId");

            migrationBuilder.RenameIndex(
                name: "IX_PosChargeInvoiceItem_ParentPosChargeInvoiceItemId",
                table: "PosChargeInvoiceItems",
                newName: "IX_PosChargeInvoiceItems_ParentPosChargeInvoiceItemId");

            migrationBuilder.RenameIndex(
                name: "IX_PosChargeInvoiceItem_OrderMenuItemId",
                table: "PosChargeInvoiceItems",
                newName: "IX_PosChargeInvoiceItems_OrderMenuItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PosChargeInvoiceItems",
                table: "PosChargeInvoiceItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosChargeInvoiceItems_OrderMenuItems_OrderMenuItemId",
                table: "PosChargeInvoiceItems",
                column: "OrderMenuItemId",
                principalTable: "OrderMenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PosChargeInvoiceItems_PosChargeInvoiceItems_ParentPosChargeInvoiceItemId",
                table: "PosChargeInvoiceItems",
                column: "ParentPosChargeInvoiceItemId",
                principalTable: "PosChargeInvoiceItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosChargeInvoiceItems_PosCharges_PosChargeId",
                table: "PosChargeInvoiceItems",
                column: "PosChargeId",
                principalTable: "PosCharges",
                principalColumn: "ChargeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PosChargeInvoiceItems_OrderMenuItems_OrderMenuItemId",
                table: "PosChargeInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PosChargeInvoiceItems_PosChargeInvoiceItems_ParentPosChargeInvoiceItemId",
                table: "PosChargeInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PosChargeInvoiceItems_PosCharges_PosChargeId",
                table: "PosChargeInvoiceItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PosChargeInvoiceItems",
                table: "PosChargeInvoiceItems");

            migrationBuilder.RenameTable(
                name: "PosChargeInvoiceItems",
                newName: "PosChargeInvoiceItem");

            migrationBuilder.RenameIndex(
                name: "IX_PosChargeInvoiceItems_PosChargeId",
                table: "PosChargeInvoiceItem",
                newName: "IX_PosChargeInvoiceItem_PosChargeId");

            migrationBuilder.RenameIndex(
                name: "IX_PosChargeInvoiceItems_ParentPosChargeInvoiceItemId",
                table: "PosChargeInvoiceItem",
                newName: "IX_PosChargeInvoiceItem_ParentPosChargeInvoiceItemId");

            migrationBuilder.RenameIndex(
                name: "IX_PosChargeInvoiceItems_OrderMenuItemId",
                table: "PosChargeInvoiceItem",
                newName: "IX_PosChargeInvoiceItem_OrderMenuItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PosChargeInvoiceItem",
                table: "PosChargeInvoiceItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosChargeInvoiceItem_OrderMenuItems_OrderMenuItemId",
                table: "PosChargeInvoiceItem",
                column: "OrderMenuItemId",
                principalTable: "OrderMenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PosChargeInvoiceItem_PosChargeInvoiceItem_ParentPosChargeInvoiceItemId",
                table: "PosChargeInvoiceItem",
                column: "ParentPosChargeInvoiceItemId",
                principalTable: "PosChargeInvoiceItem",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosChargeInvoiceItem_PosCharges_PosChargeId",
                table: "PosChargeInvoiceItem",
                column: "PosChargeId",
                principalTable: "PosCharges",
                principalColumn: "ChargeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
