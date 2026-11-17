using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Calmaria.Infrastructure.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Privacidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasEmCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Destino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mitigado = table.Column<bool>(type: "boolean", nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParaCidadao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ParaEquipe = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GeradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasEmCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventosDeFluxo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Origem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Destino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Peso = table.Column<int>(type: "integer", nullable: false),
                    Bloqueado = table.Column<bool>(type: "boolean", nullable: false),
                    OcorridoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosDeFluxo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Varreduras",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IniciadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Indice = table.Column<int>(type: "integer", nullable: false),
                    Faixa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Expostos = table.Column<int>(type: "integer", nullable: false),
                    Mitigados = table.Column<int>(type: "integer", nullable: false),
                    Fluxos = table.Column<int>(type: "integer", nullable: false),
                    ComposicaoJson = table.Column<string>(type: "text", nullable: false),
                    FontesJson = table.Column<string>(type: "text", nullable: false),
                    AlertasJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Varreduras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AchadosDeVarredura",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VarreduraId = table.Column<long>(type: "bigint", nullable: false),
                    Fonte = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Destino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Local = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RegraId = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Peso = table.Column<int>(type: "integer", nullable: false),
                    Trecho = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchadosDeVarredura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchadosDeVarredura_Varreduras_VarreduraId",
                        column: x => x.VarreduraId,
                        principalTable: "Varreduras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchadosDeVarredura_VarreduraId",
                table: "AchadosDeVarredura",
                column: "VarreduraId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasEmCache_Categoria_Destino_Mitigado_Modelo",
                table: "AlertasEmCache",
                columns: new[] { "Categoria", "Destino", "Mitigado", "Modelo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosDeFluxo_OcorridoEm",
                table: "EventosDeFluxo",
                column: "OcorridoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Varreduras_ConcluidaEm",
                table: "Varreduras",
                column: "ConcluidaEm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchadosDeVarredura");

            migrationBuilder.DropTable(
                name: "AlertasEmCache");

            migrationBuilder.DropTable(
                name: "EventosDeFluxo");

            migrationBuilder.DropTable(
                name: "Varreduras");
        }
    }
}
