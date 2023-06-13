using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mAIa.Data.Migrations
{
    /// <inheritdoc />
    public partial class Innapropriate_content_warning_counts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InappropriateWarningCount",
                schema: "Chat",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InnappropriateWarningIgnoredFrom",
                schema: "Chat",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InappropriateWarningCount",
                schema: "Chat",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InnappropriateWarningIgnoredFrom",
                schema: "Chat",
                table: "Users");
        }
    }
}
