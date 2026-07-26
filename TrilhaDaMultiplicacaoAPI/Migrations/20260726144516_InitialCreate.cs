using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrilhaDaMultiplicacaoAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alunos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false),
                    AvatarEmoji = table.Column<string>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alunos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conquistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Icone = table.Column<string>(type: "TEXT", nullable: false),
                    FasesConcluidasNecessarias = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conquistas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FasesProgresso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroFase = table.Column<int>(type: "INTEGER", nullable: false),
                    Estrelas = table.Column<int>(type: "INTEGER", nullable: false),
                    Pontos = table.Column<int>(type: "INTEGER", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FasesProgresso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FasesProgresso_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlunoConquistas",
                columns: table => new
                {
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConquistaId = table.Column<int>(type: "INTEGER", nullable: false),
                    DesbloqueadaEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlunoConquistas", x => new { x.AlunoId, x.ConquistaId });
                    table.ForeignKey(
                        name: "FK_AlunoConquistas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlunoConquistas_Conquistas_ConquistaId",
                        column: x => x.ConquistaId,
                        principalTable: "Conquistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Conquistas",
                columns: new[] { "Id", "Descricao", "FasesConcluidasNecessarias", "Icone", "Titulo" },
                values: new object[,]
                {
                    { 1, "Concluiu a primeira fase da trilha.", 1, "🌱", "Primeiros passos" },
                    { 2, "Concluiu 3 fases da trilha.", 3, "🔥", "Pegando o jeito" },
                    { 3, "Concluiu 6 fases da trilha.", 6, "🏆", "Mestre da multiplicação" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlunoConquistas_ConquistaId",
                table: "AlunoConquistas",
                column: "ConquistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_Email",
                table: "Alunos",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FasesProgresso_AlunoId_NumeroFase",
                table: "FasesProgresso",
                columns: new[] { "AlunoId", "NumeroFase" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlunoConquistas");

            migrationBuilder.DropTable(
                name: "FasesProgresso");

            migrationBuilder.DropTable(
                name: "Conquistas");

            migrationBuilder.DropTable(
                name: "Alunos");
        }
    }
}
