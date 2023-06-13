using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mAIa.Data.Migrations
{
    /// <inheritdoc />
    public partial class Added_server_and_channel_ID_to_message_storage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChannelID",
                schema: "Chat",
                table: "Messages",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ServerID",
                schema: "Chat",
                table: "Messages",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelID",
                schema: "Chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ServerID",
                schema: "Chat",
                table: "Messages");
        }
    }
}
