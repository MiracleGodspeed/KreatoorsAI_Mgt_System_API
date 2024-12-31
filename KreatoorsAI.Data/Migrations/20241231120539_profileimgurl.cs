using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KreatoorsAI.Data.Migrations
{
    /// <inheritdoc />
    public partial class profileimgurl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profileImage",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profileImage",
                table: "Users");
        }
    }
}
