using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NanTingBlog.API.Migrations
{
    /// <inheritdoc />
    public partial class 友链表 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "friendslink",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    dictum = table.Column<string>(type: "text", nullable: false),
                    avatar = table.Column<string>(type: "text", nullable: false),
                    createUnixEpochTick = table.Column<long>(type: "bigint", nullable: false),
                    createTime = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friendslink", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "friendslink");
        }
    }
}
