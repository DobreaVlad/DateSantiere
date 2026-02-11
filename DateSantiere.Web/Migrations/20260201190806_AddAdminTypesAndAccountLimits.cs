using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DateSantiere.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminTypesAndAccountLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdminType",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentMonthExports",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentMonthSearches",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastResetDate",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MonthlyExportLimit",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonthlySearchLimit",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AdminType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CurrentMonthExports",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CurrentMonthSearches",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastResetDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MonthlyExportLimit",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MonthlySearchLimit",
                table: "AspNetUsers");
        }
    }
}
