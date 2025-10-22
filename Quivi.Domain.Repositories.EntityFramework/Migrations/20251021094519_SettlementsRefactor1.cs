using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SettlementsRefactor1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_Merchants_MerchantId",
                table: "People");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementDetails_Merchants_SubMerchantId",
                table: "SettlementDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementServiceDetails_Merchants_SubMerchantId",
                table: "SettlementServiceDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementServiceDetails",
                table: "SettlementServiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_SettlementServiceDetails_SettlementId",
                table: "SettlementServiceDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementDetails",
                table: "SettlementDetails");

            migrationBuilder.DropIndex(
                name: "IX_SettlementDetails_SettlementId",
                table: "SettlementDetails");

            migrationBuilder.DropColumn(
                name: "SettlementServiceDetailId",
                table: "SettlementServiceDetails");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "SettlementDetailId",
                table: "SettlementDetails");

            migrationBuilder.DropColumn(
                name: "ChargeMethod",
                table: "SettlementDetails");

            migrationBuilder.DropColumn(
                name: "DatetimeUTC",
                table: "SettlementDetails");

            migrationBuilder.DropColumn(
                name: "SettlementDate",
                table: "SettlementDetails");

            migrationBuilder.DropColumn(
                name: "SettlementDays",
                table: "SettlementDetails");

            migrationBuilder.RenameColumn(
                name: "SubMerchantVatRate",
                table: "SettlementServiceDetails",
                newName: "MerchantVatRate");

            migrationBuilder.RenameColumn(
                name: "SubMerchantId",
                table: "SettlementServiceDetails",
                newName: "ParentMerchantId");

            migrationBuilder.RenameColumn(
                name: "SubMerchantIban",
                table: "SettlementServiceDetails",
                newName: "MerchantIban");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementServiceDetails_SubMerchantId",
                table: "SettlementServiceDetails",
                newName: "IX_SettlementServiceDetails_ParentMerchantId");

            migrationBuilder.RenameColumn(
                name: "SettlementId",
                table: "Settlements",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SubMerchantVatRate",
                table: "SettlementDetails",
                newName: "MerchantVatRate");

            migrationBuilder.RenameColumn(
                name: "SubMerchantId",
                table: "SettlementDetails",
                newName: "ParentMerchantId");

            migrationBuilder.RenameColumn(
                name: "SubMerchantIban",
                table: "SettlementDetails",
                newName: "MerchantIban");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementDetails_SubMerchantId",
                table: "SettlementDetails",
                newName: "IX_SettlementDetails_ParentMerchantId");

            migrationBuilder.RenameColumn(
                name: "MerchantId",
                table: "People",
                newName: "ParentMerchantId");

            migrationBuilder.RenameIndex(
                name: "IX_People_MerchantId",
                table: "People",
                newName: "IX_People_ParentMerchantId");

            migrationBuilder.AlterColumn<int>(
                name: "ChargeMethod",
                table: "Journals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RedeemCode",
                table: "ApiClientRequests",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceReference",
                table: "ApiClientRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementServiceDetails",
                table: "SettlementServiceDetails",
                columns: new[] { "SettlementId", "JournalId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementDetails",
                table: "SettlementDetails",
                columns: new[] { "SettlementId", "JournalId" });

            migrationBuilder.AddForeignKey(
                name: "FK_People_Merchants_ParentMerchantId",
                table: "People",
                column: "ParentMerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementDetails_Merchants_ParentMerchantId",
                table: "SettlementDetails",
                column: "ParentMerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementServiceDetails_Merchants_ParentMerchantId",
                table: "SettlementServiceDetails",
                column: "ParentMerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_Merchants_ParentMerchantId",
                table: "People");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementDetails_Merchants_ParentMerchantId",
                table: "SettlementDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementServiceDetails_Merchants_ParentMerchantId",
                table: "SettlementServiceDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementServiceDetails",
                table: "SettlementServiceDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementDetails",
                table: "SettlementDetails");

            migrationBuilder.RenameColumn(
                name: "ParentMerchantId",
                table: "SettlementServiceDetails",
                newName: "SubMerchantId");

            migrationBuilder.RenameColumn(
                name: "MerchantVatRate",
                table: "SettlementServiceDetails",
                newName: "SubMerchantVatRate");

            migrationBuilder.RenameColumn(
                name: "MerchantIban",
                table: "SettlementServiceDetails",
                newName: "SubMerchantIban");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementServiceDetails_ParentMerchantId",
                table: "SettlementServiceDetails",
                newName: "IX_SettlementServiceDetails_SubMerchantId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Settlements",
                newName: "SettlementId");

            migrationBuilder.RenameColumn(
                name: "ParentMerchantId",
                table: "SettlementDetails",
                newName: "SubMerchantId");

            migrationBuilder.RenameColumn(
                name: "MerchantVatRate",
                table: "SettlementDetails",
                newName: "SubMerchantVatRate");

            migrationBuilder.RenameColumn(
                name: "MerchantIban",
                table: "SettlementDetails",
                newName: "SubMerchantIban");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementDetails_ParentMerchantId",
                table: "SettlementDetails",
                newName: "IX_SettlementDetails_SubMerchantId");

            migrationBuilder.RenameColumn(
                name: "ParentMerchantId",
                table: "People",
                newName: "MerchantId");

            migrationBuilder.RenameIndex(
                name: "IX_People_ParentMerchantId",
                table: "People",
                newName: "IX_People_MerchantId");

            migrationBuilder.AddColumn<int>(
                name: "SettlementServiceDetailId",
                table: "SettlementServiceDetails",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "Settlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SettlementDetailId",
                table: "SettlementDetails",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "ChargeMethod",
                table: "SettlementDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatetimeUTC",
                table: "SettlementDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "SettlementDate",
                table: "SettlementDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SettlementDays",
                table: "SettlementDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ChargeMethod",
                table: "Journals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RedeemCode",
                table: "ApiClientRequests",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceReference",
                table: "ApiClientRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementServiceDetails",
                table: "SettlementServiceDetails",
                column: "SettlementServiceDetailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementDetails",
                table: "SettlementDetails",
                column: "SettlementDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementServiceDetails_SettlementId",
                table: "SettlementServiceDetails",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDetails_SettlementId",
                table: "SettlementDetails",
                column: "SettlementId");

            migrationBuilder.AddForeignKey(
                name: "FK_People_Merchants_MerchantId",
                table: "People",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementDetails_Merchants_SubMerchantId",
                table: "SettlementDetails",
                column: "SubMerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementServiceDetails_Merchants_SubMerchantId",
                table: "SettlementServiceDetails",
                column: "SubMerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
