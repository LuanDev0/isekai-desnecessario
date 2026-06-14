using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLootboxToPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataXpHoje",
                table: "Perfis",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaLootbox",
                table: "Perfis",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "XpHoje",
                table: "Perfis",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataXpHoje",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "UltimaLootbox",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "XpHoje",
                table: "Perfis");
        }
    }
}
