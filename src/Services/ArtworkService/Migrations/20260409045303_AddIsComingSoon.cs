using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtworkService.Migrations
{
    /// <inheritdoc />
    public partial class AddIsComingSoon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComingSoon",
                table: "Artworks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComingSoon",
                table: "Artworks");
        }
    }
}
