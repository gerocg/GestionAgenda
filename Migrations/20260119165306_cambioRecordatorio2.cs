using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAgenda.Migrations
{
    /// <inheritdoc />
    public partial class cambioRecordatorio2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recordatorios_Profesionales_ProfesionalId",
                table: "Recordatorios");

            migrationBuilder.DropIndex(
                name: "IX_Recordatorios_ProfesionalId",
                table: "Recordatorios");

            migrationBuilder.DropColumn(
                name: "ProfesionalId",
                table: "Recordatorios");

            migrationBuilder.AddColumn<bool>(
                name: "Enviado",
                table: "Recordatorios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnviado",
                table: "Recordatorios",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "chatId",
                table: "Recordatorios",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "fechaEnvio",
                table: "Recordatorios",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enviado",
                table: "Recordatorios");

            migrationBuilder.DropColumn(
                name: "FechaEnviado",
                table: "Recordatorios");

            migrationBuilder.DropColumn(
                name: "chatId",
                table: "Recordatorios");

            migrationBuilder.DropColumn(
                name: "fechaEnvio",
                table: "Recordatorios");

            migrationBuilder.AddColumn<int>(
                name: "ProfesionalId",
                table: "Recordatorios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Recordatorios_ProfesionalId",
                table: "Recordatorios",
                column: "ProfesionalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recordatorios_Profesionales_ProfesionalId",
                table: "Recordatorios",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
