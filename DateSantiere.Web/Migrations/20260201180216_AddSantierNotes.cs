using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DateSantiere.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSantierNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SantierNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SantierId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Nota = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Alarma = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SantierNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SantierNotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SantierNotes_Santiere_SantierId",
                        column: x => x.SantierId,
                        principalTable: "Santiere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SantierNotes_Alarma",
                table: "SantierNotes",
                column: "Alarma");

            migrationBuilder.CreateIndex(
                name: "IX_SantierNotes_SantierId_UserId",
                table: "SantierNotes",
                columns: new[] { "SantierId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SantierNotes_UserId",
                table: "SantierNotes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SantierNotes");
        }
    }
}
