using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAgenda.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguracionCalendarioBloqueoHorariosYAlgoMas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Historiales_HistorialClinicoId",
                table: "Citas");

            migrationBuilder.DropIndex(
                name: "IX_Citas_HistorialClinicoId",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "FechaRealizada",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "HistorialClinicoId",
                table: "Citas");

            migrationBuilder.CreateTable(
                name: "BloqueosHorarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaDesde = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Motivo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueosHorarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConfiguracionCalendario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HoraInicio = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    IntervaloBase = table.Column<int>(type: "int", nullable: false),
                    DuracionCita = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionCalendario", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ConfiguracionCalendario",
                columns: new[] { "Id", "DuracionCita", "HoraFin", "HoraInicio", "IntervaloBase" },
                values: new object[] { 1, 30, new TimeSpan(0, 18, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 30 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloqueosHorarios");

            migrationBuilder.DropTable(
                name: "ConfiguracionCalendario");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRealizada",
                table: "Citas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HistorialClinicoId",
                table: "Citas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Citas_HistorialClinicoId",
                table: "Citas",
                column: "HistorialClinicoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Historiales_HistorialClinicoId",
                table: "Citas",
                column: "HistorialClinicoId",
                principalTable: "Historiales",
                principalColumn: "Id");
        }
    }
}
