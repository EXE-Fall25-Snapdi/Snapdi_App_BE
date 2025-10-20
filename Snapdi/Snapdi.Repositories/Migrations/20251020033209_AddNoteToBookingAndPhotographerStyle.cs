using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteToBookingAndPhotographerStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Booking",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PhotographerStyle",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StyleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Photogra__PhotographerStyle", x => new { x.UserID, x.StyleID });
                    table.ForeignKey(
                        name: "FK__Photograp__Style__PhotographerStyle",
                        column: x => x.StyleID,
                        principalTable: "Styles",
                        principalColumn: "StyleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Photograp__UserI__PhotographerStyle",
                        column: x => x.UserID,
                        principalTable: "PhotographerProfile",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhotographerStyle_StyleID",
                table: "PhotographerStyle",
                column: "StyleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhotographerStyle");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Booking");
        }
    }
}
