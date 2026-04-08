using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientOpsPortal.Infrastructure.Data.Migrations.App
{
    /// <inheritdoc />
    public partial class RemoveServiceFromContractMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Services_ServiceId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ServiceId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Contracts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "Contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ServiceId",
                table: "Contracts",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Services_ServiceId",
                table: "Contracts",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }
    }
}
