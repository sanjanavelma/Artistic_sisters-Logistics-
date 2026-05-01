using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGPSFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGPSUpdate",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "LastLatitude",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "LastLongitude",
                table: "Assignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastGPSUpdate",
                table: "Assignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastLatitude",
                table: "Assignments",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastLongitude",
                table: "Assignments",
                type: "decimal(10,7)",
                nullable: true);
        }
    }
}
