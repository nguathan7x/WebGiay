using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebGiayOnline.Migrations
{
    /// <inheritdoc />
    public partial class ahihi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderDetailId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderDetailId1",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_OrderDetailId",
                table: "Reviews",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_OrderDetailId1",
                table: "Reviews",
                column: "OrderDetailId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_OrderDetails_OrderDetailId",
                table: "Reviews",
                column: "OrderDetailId",
                principalTable: "OrderDetails",
                principalColumn: "OrderDetailId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_OrderDetails_OrderDetailId1",
                table: "Reviews",
                column: "OrderDetailId1",
                principalTable: "OrderDetails",
                principalColumn: "OrderDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_OrderDetails_OrderDetailId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_OrderDetails_OrderDetailId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_OrderDetailId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_OrderDetailId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "OrderDetailId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "OrderDetailId1",
                table: "Reviews");
        }
    }
}
