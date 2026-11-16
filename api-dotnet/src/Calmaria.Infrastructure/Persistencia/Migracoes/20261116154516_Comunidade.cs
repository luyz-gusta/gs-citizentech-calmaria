using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Calmaria.Infrastructure.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Comunidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Apelido = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerfisSensoriais",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    SensibilidadeSom = table.Column<int>(type: "integer", nullable: false),
                    SensibilidadeLuz = table.Column<int>(type: "integer", nullable: false),
                    SensibilidadeMultidao = table.Column<int>(type: "integer", nullable: false),
                    ContatoApoioNome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ContatoApoioTelefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisSensoriais", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_PerfisSensoriais_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Relatos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    LugarId = table.Column<int>(type: "integer", nullable: false),
                    Nota = table.Column<int>(type: "integer", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VisitadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relatos_Lugares_LugarId",
                        column: x => x.LugarId,
                        principalTable: "Lugares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relatos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatoGatilhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Gatilho = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RelatoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatoGatilhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelatoGatilhos_Relatos_RelatoId",
                        column: x => x.RelatoId,
                        principalTable: "Relatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelatoGatilhos_RelatoId",
                table: "RelatoGatilhos",
                column: "RelatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Relatos_LugarId_CriadoEm",
                table: "Relatos",
                columns: new[] { "LugarId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Relatos_UsuarioId_LugarId_CriadoEm",
                table: "Relatos",
                columns: new[] { "UsuarioId", "LugarId", "CriadoEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfisSensoriais");

            migrationBuilder.DropTable(
                name: "RelatoGatilhos");

            migrationBuilder.DropTable(
                name: "Relatos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
