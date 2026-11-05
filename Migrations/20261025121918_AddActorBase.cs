using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackMap.Migrations
{
    /// <inheritdoc />
    public partial class AddActorBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseLabel",
                table: "PublicActor",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseLatitude",
                table: "PublicActor",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseLongitude",
                table: "PublicActor",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseLabel",
                table: "PublicActor");

            migrationBuilder.DropColumn(
                name: "BaseLatitude",
                table: "PublicActor");

            migrationBuilder.DropColumn(
                name: "BaseLongitude",
                table: "PublicActor");
        }
    }
}
