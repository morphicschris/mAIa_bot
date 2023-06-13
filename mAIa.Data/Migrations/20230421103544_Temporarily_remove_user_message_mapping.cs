using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mAIa.Data.Migrations
{
    /// <inheritdoc />
    public partial class Temporarily_remove_user_message_mapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_DiscordUserID",
                schema: "Chat",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DiscordUserID",
                schema: "Chat",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Messages_DiscordUserID",
                schema: "Chat",
                table: "Messages",
                column: "DiscordUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_DiscordUserID",
                schema: "Chat",
                table: "Messages",
                column: "DiscordUserID",
                principalSchema: "Chat",
                principalTable: "Users",
                principalColumn: "DiscordUserID");
        }
    }
}
