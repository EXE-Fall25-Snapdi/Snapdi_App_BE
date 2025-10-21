using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoPricePhotoTypeWorkLocationToPhotographerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PhotoPrice",
                table: "PhotographerProfile",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoType",
                table: "PhotographerProfile",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkLocation",
                table: "PhotographerProfile",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPrice",
                table: "PhotographerProfile");

            migrationBuilder.DropColumn(
                name: "PhotoType",
                table: "PhotographerProfile");

            migrationBuilder.DropColumn(
                name: "WorkLocation",
                table: "PhotographerProfile");
        }
    }
}
