using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStyleFromBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Booking__StyleID__5535A963",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_StyleID",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "StyleID",
                table: "Booking");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StyleID",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Booking_StyleID",
                table: "Booking",
                column: "StyleID");

            migrationBuilder.AddForeignKey(
                name: "FK__Booking__StyleID__5535A963",
                table: "Booking",
                column: "StyleID",
                principalTable: "Styles",
                principalColumn: "StyleID");
        }
    }
}
