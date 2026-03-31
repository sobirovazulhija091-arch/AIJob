using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConversationReadCursors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "User1LastReadMessageId",
                table: "Conversations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "User2LastReadMessageId",
                table: "Conversations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User1LastReadMessageId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "User2LastReadMessageId",
                table: "Conversations");
        }
    }
}
