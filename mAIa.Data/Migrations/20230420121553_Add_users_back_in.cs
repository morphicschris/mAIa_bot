using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mAIa.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_users_back_in : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Chat",
                columns: table => new
                {
                    DiscordUserID = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    DiscordUsername = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.DiscordUserID);
                });

            migrationBuilder.CreateTable(
                name: "UserTraits",
                schema: "Chat",
                columns: table => new
                {
                    UserTraitID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscordUserID = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelevanceToUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTraits", x => x.UserTraitID);
                    table.ForeignKey(
                        name: "FK_UserTraits_Users_DiscordUserID",
                        column: x => x.DiscordUserID,
                        principalSchema: "Chat",
                        principalTable: "Users",
                        principalColumn: "DiscordUserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DiscordUserID",
                schema: "Chat",
                table: "Messages",
                column: "DiscordUserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTraits_DiscordUserID",
                schema: "Chat",
                table: "UserTraits",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_DiscordUserID",
                schema: "Chat",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "UserTraits",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Chat");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DiscordUserID",
                schema: "Chat",
                table: "Messages");
        }
    }
}
