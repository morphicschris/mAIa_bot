using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mAIa.Data.Migrations
{
    /// <inheritdoc />
    public partial class Plain_text_message_content : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlainText",
                schema: "Chat",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlainText",
                schema: "Chat",
                table: "Messages");
        }
    }
}
