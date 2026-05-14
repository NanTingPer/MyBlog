using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NanTingBlog.API.Migrations
{
    /// <inheritdoc />
    public partial class Posttableaddtitleanddescriptionfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "posts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "title",
                table: "posts");
        }
    }
}
