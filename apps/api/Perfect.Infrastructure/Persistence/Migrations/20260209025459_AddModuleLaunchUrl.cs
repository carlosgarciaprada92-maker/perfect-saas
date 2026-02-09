using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Perfect.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleLaunchUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LaunchUrl",
                table: "ModuleCatalogs",
                type: "character varying(700)",
                maxLength: 700,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaunchUrl",
                table: "ModuleCatalogs");
        }
    }
}
