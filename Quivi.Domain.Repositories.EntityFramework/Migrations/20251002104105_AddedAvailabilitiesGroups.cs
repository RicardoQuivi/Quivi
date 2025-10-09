using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedAvailabilitiesGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItemWeeklyAvailabilities");

            migrationBuilder.CreateTable(
                name: "Availabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Availabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Availabilities_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilityMenuItemAssociation",
                columns: table => new
                {
                    AvailabilityGroupId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityMenuItemAssociation", x => new { x.AvailabilityGroupId, x.MenuItemId });
                    table.ForeignKey(
                        name: "FK_AvailabilityMenuItemAssociation_Availabilities_AvailabilityGroupId",
                        column: x => x.AvailabilityGroupId,
                        principalTable: "Availabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvailabilityMenuItemAssociation_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilityProfileAssociation",
                columns: table => new
                {
                    AvailabilityGroupId = table.Column<int>(type: "int", nullable: false),
                    ChannelProfileId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityProfileAssociation", x => new { x.AvailabilityGroupId, x.ChannelProfileId });
                    table.ForeignKey(
                        name: "FK_AvailabilityProfileAssociation_Availabilities_AvailabilityGroupId",
                        column: x => x.AvailabilityGroupId,
                        principalTable: "Availabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvailabilityProfileAssociation_ChannelProfile_ChannelProfileId",
                        column: x => x.ChannelProfileId,
                        principalTable: "ChannelProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyAvailability",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartAtSeconds = table.Column<int>(type: "int", nullable: false),
                    EndAtSeconds = table.Column<int>(type: "int", nullable: false),
                    AvailabilityGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyAvailability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyAvailability_Availabilities_AvailabilityGroupId",
                        column: x => x.AvailabilityGroupId,
                        principalTable: "Availabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_MerchantId",
                table: "Availabilities",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityMenuItemAssociation_MenuItemId",
                table: "AvailabilityMenuItemAssociation",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityProfileAssociation_ChannelProfileId",
                table: "AvailabilityProfileAssociation",
                column: "ChannelProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAvailability_AvailabilityGroupId",
                table: "WeeklyAvailability",
                column: "AvailabilityGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityMenuItemAssociation");

            migrationBuilder.DropTable(
                name: "AvailabilityProfileAssociation");

            migrationBuilder.DropTable(
                name: "WeeklyAvailability");

            migrationBuilder.DropTable(
                name: "Availabilities");

            migrationBuilder.CreateTable(
                name: "MenuItemWeeklyAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndAtSeconds = table.Column<int>(type: "int", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartAtSeconds = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemWeeklyAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemWeeklyAvailabilities_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemWeeklyAvailabilities_MenuItemId",
                table: "MenuItemWeeklyAvailabilities",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "MenuItemWeeklyAvailabilities",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");
        }
    }
}
