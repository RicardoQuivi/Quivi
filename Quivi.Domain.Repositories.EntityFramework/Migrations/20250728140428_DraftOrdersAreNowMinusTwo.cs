using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class DraftOrdersAreNowMinusTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE
                    Orders
                SET
                    State = -3
                WHERE
                    State = -2;
            ");
            migrationBuilder.Sql(@"
                UPDATE
                    Orders
                SET
                    State = -2
                WHERE
                    State = -1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE
                    Orders
                SET
                    State = -1
                WHERE
                    State = -2;
            ");
            migrationBuilder.Sql(@"
                UPDATE
                    Orders
                SET
                    State = -2
                WHERE
                    State = -3;
            ");
        }
    }
}