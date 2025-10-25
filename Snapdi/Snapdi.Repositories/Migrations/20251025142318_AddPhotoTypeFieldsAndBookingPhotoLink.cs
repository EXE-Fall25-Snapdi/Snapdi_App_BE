using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoTypeFieldsAndBookingPhotoLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPrice",
                table: "PhotographerProfile");

            migrationBuilder.AddColumn<double>(
                name: "PhotoPrice",
                table: "PhotoType",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Time",
                table: "PhotoType",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoLink",
                table: "Booking",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPrice",
                table: "PhotoType");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "PhotoType");

            migrationBuilder.DropColumn(
                name: "PhotoLink",
                table: "Booking");

            migrationBuilder.AddColumn<decimal>(
                name: "PhotoPrice",
                table: "PhotographerProfile",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
