using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebGiayOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarPublicIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPublicId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPublicId",
                table: "AspNetUsers");
        }
    }
}
