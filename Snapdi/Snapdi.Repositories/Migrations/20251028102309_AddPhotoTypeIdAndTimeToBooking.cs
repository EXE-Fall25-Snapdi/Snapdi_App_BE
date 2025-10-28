using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoTypeIdAndTimeToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoTypeID",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Time",
                table: "Booking",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoTypeID",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "Booking");
        }
    }
}
