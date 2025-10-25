using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class MovePhotoTypeFieldsToPhotographerPhotoType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPrice",
                table: "PhotoType");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "PhotoType");

            migrationBuilder.AddColumn<double>(
                name: "PhotoPrice",
                table: "PhotographerPhotoType",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Time",
                table: "PhotographerPhotoType",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPrice",
                table: "PhotographerPhotoType");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "PhotographerPhotoType");

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
        }
    }
}
