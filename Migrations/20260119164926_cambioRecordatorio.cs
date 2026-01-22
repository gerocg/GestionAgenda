using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAgenda.Migrations
{
    /// <inheritdoc />
    public partial class cambioRecordatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "Recordatorios");

            migrationBuilder.AddColumn<int>(
                name: "citaId",
                table: "Recordatorios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Recordatorios_citaId",
                table: "Recordatorios",
                column: "citaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recordatorios_Citas_citaId",
                table: "Recordatorios",
                column: "citaId",
                principalTable: "Citas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recordatorios_Citas_citaId",
                table: "Recordatorios");

            migrationBuilder.DropIndex(
                name: "IX_Recordatorios_citaId",
                table: "Recordatorios");

            migrationBuilder.DropColumn(
                name: "citaId",
                table: "Recordatorios");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "Recordatorios",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
