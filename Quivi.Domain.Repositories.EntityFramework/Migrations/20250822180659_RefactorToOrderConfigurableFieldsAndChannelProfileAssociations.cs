using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToOrderConfigurableFieldsAndChannelProfileAssociations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderConfigurableField_ChannelProfile_ChannelProfileId",
                table: "OrderConfigurableField");

            migrationBuilder.RenameColumn(
                name: "ChannelProfileId",
                table: "OrderConfigurableField",
                newName: "MerchantId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderConfigurableField_ChannelProfileId",
                table: "OrderConfigurableField",
                newName: "IX_OrderConfigurableField_MerchantId");

            migrationBuilder.CreateTable(
                name: "OrderConfigurableFieldChannelProfileAssociation",
                columns: table => new
                {
                    OrderConfigurableFieldId = table.Column<int>(type: "int", nullable: false),
                    ChannelProfileId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderConfigurableFieldChannelProfileAssociation", x => new { x.OrderConfigurableFieldId, x.ChannelProfileId });
                    table.ForeignKey(
                        name: "FK_OrderConfigurableFieldChannelProfileAssociation_ChannelProfile_ChannelProfileId",
                        column: x => x.ChannelProfileId,
                        principalTable: "ChannelProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderConfigurableFieldChannelProfileAssociation_OrderConfigurableField_OrderConfigurableFieldId",
                        column: x => x.OrderConfigurableFieldId,
                        principalTable: "OrderConfigurableField",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderConfigurableFieldChannelProfileAssociation_ChannelProfileId",
                table: "OrderConfigurableFieldChannelProfileAssociation",
                column: "ChannelProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderConfigurableField_Merchants_MerchantId",
                table: "OrderConfigurableField",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderConfigurableField_Merchants_MerchantId",
                table: "OrderConfigurableField");

            migrationBuilder.DropTable(
                name: "OrderConfigurableFieldChannelProfileAssociation");

            migrationBuilder.RenameColumn(
                name: "MerchantId",
                table: "OrderConfigurableField",
                newName: "ChannelProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderConfigurableField_MerchantId",
                table: "OrderConfigurableField",
                newName: "IX_OrderConfigurableField_ChannelProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderConfigurableField_ChannelProfile_ChannelProfileId",
                table: "OrderConfigurableField",
                column: "ChannelProfileId",
                principalTable: "ChannelProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
