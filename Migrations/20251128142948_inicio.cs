using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAgenda.Migrations
{
    /// <inheritdoc />
    public partial class inicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Historiales",
                columns: table => new
                {
                    id_hc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historiales", x => x.id_hc);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Profesionales",
                columns: table => new
                {
                    usuario_profesional = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contrasenia_profesional = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre_completo_profesional = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    profesion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesionales", x => x.usuario_profesional);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    id_paciente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    usuario_paciente = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contrasenia_paciente = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre_completo_paciente = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    direccion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_nacimiento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    telefono = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    historial_clinicoid_hc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.id_paciente);
                    table.ForeignKey(
                        name: "FK_Pacientes_Historiales_historial_clinicoid_hc",
                        column: x => x.historial_clinicoid_hc,
                        principalTable: "Historiales",
                        principalColumn: "id_hc",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    id_cita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idHistorialClinico = table.Column<int>(type: "int", nullable: false),
                    hcid_hc = table.Column<int>(type: "int", nullable: false),
                    idProfesional = table.Column<int>(type: "int", nullable: false),
                    profesionalusuario_profesional = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fechaAgendado = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fechaRealizada = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    comentario = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Recordatorios",
                columns: table => new
                {
                    id_recordatorio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    pacienteid_paciente = table.Column<int>(type: "int", nullable: false),
                    profesionalusuario_profesional = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mensaje = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fechaEntrega = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recordatorios", x => x.id_recordatorio);
                    table.ForeignKey(
                        name: "FK_Recordatorios_Pacientes_pacienteid_paciente",
                        column: x => x.pacienteid_paciente,
                        principalTable: "Pacientes",
                        principalColumn: "id_paciente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recordatorios_Profesionales_profesionalusuario_profesional",
                        column: x => x.profesionalusuario_profesional,
                        principalTable: "Profesionales",
                        principalColumn: "usuario_profesional");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Archivos",
                columns: table => new
                {
                    id_archivo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idCita = table.Column<int>(type: "int", nullable: false),
                    citaid_cita = table.Column<int>(type: "int", nullable: false),
                    rutaArchivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombreARchivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipoArchivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fechaSubida = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "IX_Pacientes_historial_clinicoid_hc",
                table: "Pacientes",
                column: "historial_clinicoid_hc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_usuario_paciente",
                table: "Pacientes",
                column: "usuario_paciente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recordatorios_pacienteid_paciente",
                table: "Recordatorios",
                column: "pacienteid_paciente");

            migrationBuilder.CreateIndex(
                name: "IX_Recordatorios_profesionalusuario_profesional",
                table: "Recordatorios",
                column: "profesionalusuario_profesional");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Archivos");

            migrationBuilder.DropTable(
                name: "Recordatorios");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "Profesionales");

            migrationBuilder.DropTable(
                name: "Historiales");
        }
    }
}
