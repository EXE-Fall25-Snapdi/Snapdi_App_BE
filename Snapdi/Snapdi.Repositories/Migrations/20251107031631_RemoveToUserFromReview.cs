using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapdi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveToUserFromReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Review__ToUserID__5AEE82B9",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Review_ToUserID",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "ToUserID",
                table: "Review");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToUserID",
                table: "Review",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Review_ToUserID",
                table: "Review",
                column: "ToUserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Review__ToUserID__5AEE82B9",
                table: "Review",
                column: "ToUserID",
                principalTable: "User",
                principalColumn: "UserID");
        }
    }
}
