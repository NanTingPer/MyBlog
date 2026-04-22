using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NanTingBlog.API.Migrations
{
    /// <inheritdoc />
    public partial class 初始化 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    createTime = table.Column<long>(type: "bigint", nullable: false),
                    editTime = table.Column<long>(type: "bigint", nullable: false),
                    author = table.Column<List<string>>(type: "text[]", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    tag = table.Column<List<string>>(type: "text[]", nullable: true),
                    drawingUrl = table.Column<string>(type: "text", nullable: true),
                    html = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posts");
        }
    }
}
