using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Calmaria.Infrastructure.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial_Sensorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lugares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Distrito = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    HorarioFuncionamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lugares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrevisoesBarulho",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Distrito = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    ReclamacoesPrevistas = table.Column<double>(type: "double precision", nullable: false),
                    Nivel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrevisoesBarulho", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dispositivos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    LugarId = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UltimaLeituraEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispositivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispositivos_Lugares_LugarId",
                        column: x => x.LugarId,
                        principalTable: "Lugares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leituras",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DispositivoId = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    LugarId = table.Column<int>(type: "integer", nullable: false),
                    SomDb = table.Column<double>(type: "double precision", nullable: false),
                    LuzLux = table.Column<double>(type: "double precision", nullable: false),
                    MovimentosMin = table.Column<int>(type: "integer", nullable: false),
                    Indice = table.Column<int>(type: "integer", nullable: false),
                    Nivel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RegistradaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leituras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leituras_Dispositivos_DispositivoId",
                        column: x => x.DispositivoId,
                        principalTable: "Dispositivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leituras_Lugares_LugarId",
                        column: x => x.LugarId,
                        principalTable: "Lugares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Lugares",
                columns: new[] { "Id", "Categoria", "Distrito", "Endereco", "HorarioFuncionamento", "Latitude", "Longitude", "Nome", "Tipo" },
                values: new object[,]
                {
                    { 1, "MONITORADO", "CONSOLACAO", "Av. Paulista com Rua da Consolação", "04:40-00:00", -23.557500000000001, -46.660899999999998, "Estação Consolação (Linha 2)", "Estacao" },
                    { 2, "MONITORADO", "CONSOLACAO", "Rua da Consolação com Av. Paulista", "04:40-00:00", -23.555299999999999, -46.662199999999999, "Estação Paulista (Linha 4)", "Estacao" },
                    { 3, "MONITORADO", "BELA VISTA", "Av. Paulista, em frente ao MASP", "04:40-00:00", -23.561399999999999, -46.656599999999997, "Estação Trianon-Masp (Linha 2)", "Estacao" },
                    { 4, "MONITORADO", "CONSOLACAO", "Rua Augusta (o mercado é escolhido na observação)", "07:00-23:00", -23.556000000000001, -46.658000000000001, "Supermercado da Rua Augusta", "Mercado" }
                });

            migrationBuilder.InsertData(
                table: "Lugares",
                columns: new[] { "Id", "Categoria", "Descricao", "Distrito", "Endereco", "HorarioFuncionamento", "Latitude", "Longitude", "Nome", "Tipo" },
                values: new object[,]
                {
                    { 5, "REFUGIO", "Parque com sombra, bancos e menos barulho que a Avenida Paulista.", "BELA VISTA", "Rua Peixoto Gomide, 949", "06:00-18:00", -23.561699999999998, -46.657299999999999, "Parque Trianon", "Refugio" },
                    { 6, "REFUGIO", "Casa e jardim de um centro cultural, com áreas quietas para sentar.", "BELA VISTA", "Av. Paulista, 37", null, -23.571200000000001, -46.644599999999997, "Casa das Rosas", "Refugio" }
                });

            migrationBuilder.InsertData(
                table: "Dispositivos",
                columns: new[] { "Id", "Ativo", "LugarId", "UltimaLeituraEm" },
                values: new object[,]
                {
                    { "esp32-consolacao-01", true, 1, null },
                    { "esp32-mercado-01", true, 4, null },
                    { "esp32-paulista-01", true, 2, null },
                    { "esp32-trianon-01", true, 3, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispositivos_LugarId",
                table: "Dispositivos",
                column: "LugarId");

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_DispositivoId",
                table: "Leituras",
                column: "DispositivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_LugarId_RegistradaEm",
                table: "Leituras",
                columns: new[] { "LugarId", "RegistradaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Lugares_Distrito",
                table: "Lugares",
                column: "Distrito");

            migrationBuilder.CreateIndex(
                name: "IX_PrevisoesBarulho_Distrito_Data",
                table: "PrevisoesBarulho",
                columns: new[] { "Distrito", "Data" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leituras");

            migrationBuilder.DropTable(
                name: "PrevisoesBarulho");

            migrationBuilder.DropTable(
                name: "Dispositivos");

            migrationBuilder.DropTable(
                name: "Lugares");
        }
    }
}
