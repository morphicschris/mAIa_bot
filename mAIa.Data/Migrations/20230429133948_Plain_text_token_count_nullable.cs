using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mAIa.Data.Migrations
{
    /// <inheritdoc />
    public partial class Plain_text_token_count_nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PlainTextTokenCount",
                schema: "Chat",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"UPDATE [Chat].[Messages] SET [PlainTextTokenCount] = NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PlainTextTokenCount",
                schema: "Chat",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
