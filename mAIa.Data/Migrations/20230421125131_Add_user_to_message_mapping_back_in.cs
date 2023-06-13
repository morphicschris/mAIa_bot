using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mAIa.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_user_to_message_mapping_back_in : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                principalColumn: "DiscordUserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
