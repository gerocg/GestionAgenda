using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAgenda.Migrations
{
    /// <inheritdoc />
    public partial class Inicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    usuario_paciente = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    contrasenia_paciente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nombre_completo_paciente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.usuario_paciente);
                });

            migrationBuilder.CreateTable(
                name: "Profesionales",
                columns: table => new
                {
                    usuario_profesional = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    contrasenia_profesional = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nombre_completo_profesional = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    profesion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesionales", x => x.usuario_profesional);
                });

            migrationBuilder.CreateTable(
                name: "Historiales",
                columns: table => new
                {
                    id_hc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pacienteusuario_paciente = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historiales", x => x.id_hc);
                    table.ForeignKey(
                        name: "FK_Historiales_Pacientes_pacienteusuario_paciente",
                        column: x => x.pacienteusuario_paciente,
                        principalTable: "Pacientes",
                        principalColumn: "usuario_paciente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Telefonos",
                columns: table => new
                {
                    numero = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Pacienteusuario_paciente = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Telefonos", x => x.numero);
                    table.ForeignKey(
                        name: "FK_Telefonos_Pacientes_Pacienteusuario_paciente",
                        column: x => x.Pacienteusuario_paciente,
                        principalTable: "Pacientes",
                        principalColumn: "usuario_paciente");
                });

            migrationBuilder.CreateTable(
                name: "Recordatorios",
                columns: table => new
                {
                    id_recordatorio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pacienteusuario_paciente = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    profesionalusuario_profesional = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recordatorios", x => x.id_recordatorio);
                    table.ForeignKey(
                        name: "FK_Recordatorios_Pacientes_pacienteusuario_paciente",
                        column: x => x.pacienteusuario_paciente,
                        principalTable: "Pacientes",
                        principalColumn: "usuario_paciente");
                    table.ForeignKey(
                        name: "FK_Recordatorios_Profesionales_profesionalusuario_profesional",
                        column: x => x.profesionalusuario_profesional,
                        principalTable: "Profesionales",
                        principalColumn: "usuario_profesional");
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    id_cita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hcid_hc = table.Column<int>(type: "int", nullable: false),
                    profesionalusuario_profesional = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    fechaAgendado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fechaRealizada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    comentario = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.id_cita);
                    table.ForeignKey(
                        name: "FK_Citas_Historiales_hcid_hc",
                        column: x => x.hcid_hc,
                        principalTable: "Historiales",
                        principalColumn: "id_hc",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Citas_Profesionales_profesionalusuario_profesional",
                        column: x => x.profesionalusuario_profesional,
                        principalTable: "Profesionales",
                        principalColumn: "usuario_profesional",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Archivos",
                columns: table => new
                {
                    id_archivo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    citaid_cita = table.Column<int>(type: "int", nullable: false),
                    rutaArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archivos", x => x.id_archivo);
                    table.ForeignKey(
                        name: "FK_Archivos_Citas_citaid_cita",
                        column: x => x.citaid_cita,
                        principalTable: "Citas",
                        principalColumn: "id_cita",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Archivos_citaid_cita",
                table: "Archivos",
                column: "citaid_cita");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_hcid_hc",
                table: "Citas",
                column: "hcid_hc");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_profesionalusuario_profesional",
                table: "Citas",
                column: "profesionalusuario_profesional");

            migrationBuilder.CreateIndex(
                name: "IX_Historiales_pacienteusuario_paciente",
                table: "Historiales",
                column: "pacienteusuario_paciente");

            migrationBuilder.CreateIndex(
                name: "IX_Recordatorios_pacienteusuario_paciente",
                table: "Recordatorios",
                column: "pacienteusuario_paciente");

            migrationBuilder.CreateIndex(
                name: "IX_Recordatorios_profesionalusuario_profesional",
                table: "Recordatorios",
                column: "profesionalusuario_profesional");

            migrationBuilder.CreateIndex(
                name: "IX_Telefonos_Pacienteusuario_paciente",
                table: "Telefonos",
                column: "Pacienteusuario_paciente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Archivos");

            migrationBuilder.DropTable(
                name: "Recordatorios");

            migrationBuilder.DropTable(
                name: "Telefonos");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Historiales");

            migrationBuilder.DropTable(
                name: "Profesionales");

            migrationBuilder.DropTable(
                name: "Pacientes");
        }
    }
}
