using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelPhotographerToPhotographerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LevelPhotographer",
                table: "PhotographerProfile",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelPhotographer",
                table: "PhotographerProfile");
        }
    }
}
