using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NanTingBlog.API.Migrations
{
    /// <inheritdoc />
    public partial class 用户表添加邮箱字段 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mailAddress",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mailAddress",
                table: "user");
        }
    }
}
