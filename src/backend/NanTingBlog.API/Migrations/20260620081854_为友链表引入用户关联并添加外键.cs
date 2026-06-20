using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NanTingBlog.API.Migrations
{
    /// <inheritdoc />
    public partial class 为友链表引入用户关联并添加外键 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "delete",
                table: "friendslink",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "failingText",
                table: "friendslink",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "friendslink",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "userId",
                table: "friendslink",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_friendslink_userId",
                table: "friendslink",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_friendslink_user_userId",
                table: "friendslink",
                column: "userId",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_friendslink_user_userId",
                table: "friendslink");

            migrationBuilder.DropIndex(
                name: "IX_friendslink_userId",
                table: "friendslink");

            migrationBuilder.DropColumn(
                name: "delete",
                table: "friendslink");

            migrationBuilder.DropColumn(
                name: "failingText",
                table: "friendslink");

            migrationBuilder.DropColumn(
                name: "state",
                table: "friendslink");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "friendslink");
        }
    }
}
