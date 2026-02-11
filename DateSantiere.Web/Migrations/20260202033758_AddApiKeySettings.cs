using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DateSantiere.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KeyName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    KeyValue = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeySettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeySettings");
        }
    }
}
