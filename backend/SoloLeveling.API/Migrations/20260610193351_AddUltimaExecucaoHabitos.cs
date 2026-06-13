using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoloLeveling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUltimaExecucaoHabitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaExecucao",
                table: "MausHabitos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaExecucao",
                table: "BonsHabitos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimaExecucao",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "UltimaExecucao",
                table: "BonsHabitos");
        }
    }
}
