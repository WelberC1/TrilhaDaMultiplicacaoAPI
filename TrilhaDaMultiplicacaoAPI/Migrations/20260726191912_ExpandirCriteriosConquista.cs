using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrilhaDaMultiplicacaoAPI.Migrations
{
    /// <inheritdoc />
    public partial class ExpandirCriteriosConquista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FasesConcluidasNecessarias",
                table: "Conquistas",
                newName: "ValorNecessario");

            migrationBuilder.AddColumn<int>(
                name: "TipoCriterio",
                table: "Conquistas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Descricao", "Icone", "TipoCriterio", "Titulo" },
                values: new object[] { "Complete a sua primeira fase da trilha.", "🥇", 0, "Primeiro Passo" });

            migrationBuilder.UpdateData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Descricao", "Icone", "TipoCriterio", "Titulo", "ValorNecessario" },
                values: new object[] { "Consiga 3 estrelas em pelo menos uma fase.", "🌟", 1, "Trinca de Ouro", 1 });

            migrationBuilder.UpdateData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Descricao", "Icone", "TipoCriterio", "Titulo", "ValorNecessario" },
                values: new object[] { "Complete 3 fases da trilha.", "🔥", 0, "Sequência de Craque", 3 });

            migrationBuilder.InsertData(
                table: "Conquistas",
                columns: new[] { "Id", "Descricao", "Icone", "TipoCriterio", "Titulo", "ValorNecessario" },
                values: new object[,]
                {
                    { 4, "Complete 6 fases da trilha.", "🏃", 0, "Meio Caminho", 6 },
                    { 5, "Complete as 12 fases da trilha da multiplicação.", "🏆", 0, "Trilha Completa", 12 },
                    { 6, "Consiga 3 estrelas em 5 fases diferentes.", "✨", 1, "Estrela em Dobro", 5 },
                    { 7, "Acumule 300 pontos.", "💰", 2, "Colecionador de Pontos", 300 },
                    { 8, "Acumule 800 pontos.", "👑", 2, "Mestre da Trilha", 800 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "TipoCriterio",
                table: "Conquistas");

            migrationBuilder.RenameColumn(
                name: "ValorNecessario",
                table: "Conquistas",
                newName: "FasesConcluidasNecessarias");

            migrationBuilder.UpdateData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Descricao", "Icone", "Titulo" },
                values: new object[] { "Concluiu a primeira fase da trilha.", "🌱", "Primeiros passos" });

            migrationBuilder.UpdateData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Descricao", "FasesConcluidasNecessarias", "Icone", "Titulo" },
                values: new object[] { "Concluiu 3 fases da trilha.", 3, "🔥", "Pegando o jeito" });

            migrationBuilder.UpdateData(
                table: "Conquistas",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Descricao", "FasesConcluidasNecessarias", "Icone", "Titulo" },
                values: new object[] { "Concluiu 6 fases da trilha.", 6, "🏆", "Mestre da multiplicação" });
        }
    }
}
