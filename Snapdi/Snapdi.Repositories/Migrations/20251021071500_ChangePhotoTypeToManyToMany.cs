using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ChangePhotoTypeToManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoType",
                table: "PhotographerProfile");

            migrationBuilder.CreateTable(
                name: "PhotoType",
                columns: table => new
                {
                    PhotoTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhotoTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PhotoTyp__8AD147A0C035264E", x => x.PhotoTypeID);
                });

            migrationBuilder.CreateTable(
                name: "PhotographerPhotoType",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PhotoTypeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Photogra__PhotographerPhotoType", x => new { x.UserID, x.PhotoTypeID });
                    table.ForeignKey(
                        name: "FK__Photograp__Photo__PhotographerPhotoType",
                        column: x => x.PhotoTypeID,
                        principalTable: "PhotoType",
                        principalColumn: "PhotoTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Photograp__UserI__PhotographerPhotoType",
                        column: x => x.UserID,
                        principalTable: "PhotographerProfile",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhotographerPhotoType_PhotoTypeID",
                table: "PhotographerPhotoType",
                column: "PhotoTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhotographerPhotoType");

            migrationBuilder.DropTable(
                name: "PhotoType");

            migrationBuilder.AddColumn<string>(
                name: "PhotoType",
                table: "PhotographerProfile",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
