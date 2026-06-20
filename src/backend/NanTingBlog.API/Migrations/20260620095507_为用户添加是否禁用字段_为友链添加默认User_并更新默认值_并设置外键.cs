using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NanTingBlog.API.Migrations
{
    /// <inheritdoc />
    public partial class 为用户添加是否禁用字段_为友链添加默认User_并更新默认值_并设置外键 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isBanned",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                defaultValue: "a0000000-0000-0000-0000-000000000001");

            migrationBuilder.InsertData(
                table: "user",
                columns: new[] { "id", "createTime", "isBanned", "mailAddress", "name", "password", "roles" },
                values: new object[] { "a0000000-0000-0000-0000-000000000001", 0L, true, "system@localhost", "default", "", new[] { 1 } });

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

            migrationBuilder.DeleteData(
                table: "user",
                keyColumn: "id",
                keyValue: "a0000000-0000-0000-0000-000000000001");

            migrationBuilder.DropColumn(
                name: "isBanned",
                table: "user");

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
