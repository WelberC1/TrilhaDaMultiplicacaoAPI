using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrilhaDaMultiplicacaoAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNomeUsuarioToAluno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeUsuario",
                table: "Alunos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_NomeUsuario",
                table: "Alunos",
                column: "NomeUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alunos_NomeUsuario",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "NomeUsuario",
                table: "Alunos");
        }
    }
}
