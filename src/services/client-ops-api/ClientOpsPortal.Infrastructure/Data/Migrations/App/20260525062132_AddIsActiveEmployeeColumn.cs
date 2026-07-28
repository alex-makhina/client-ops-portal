using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientOpsPortal.Infrastructure.Data.Migrations.App
{
    /// <inheritdoc />
    public partial class AddIsActiveEmployeeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employees");
        }
    }
}
