using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAgenda.Migrations
{
    /// <inheritdoc />
    public partial class recordatoriosEnCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Recordatorio24hEnviado",
                table: "Citas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Recordatorio2hEnviado",
                table: "Citas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recordatorio24hEnviado",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "Recordatorio2hEnviado",
                table: "Citas");
        }
    }
}
