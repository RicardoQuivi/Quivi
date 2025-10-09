using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedAvailabilityFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE FUNCTION fn_ToTimeZone(
	                @date DATETIME,
	                @timezone NVARCHAR(50)
                )
                RETURNS DATETIMEOFFSET AS
                BEGIN
                    RETURN @date AT TIME ZONE 'UTC' AT TIME ZONE @timezone;
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE FUNCTION [dbo].[fn_ToWeeklyAvailabilityInSeconds](
	                @dateOffset DATETIMEOFFSET
                )
                RETURNS INT AS
                BEGIN
	                DECLARE @date DATETIME = CAST(@dateOffset AS DATETIME)
                    RETURN DATEDIFF(SS, DATEADD(DAY, -1*(DATEPART (DW , @date) - 1), CAST(FLOOR(CAST(@date AS FLOAT)) AS DATETIME)), @date)
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_ToTimeZone");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_ToWeeklyAvailabilityInSeconds");
        }
    }
}