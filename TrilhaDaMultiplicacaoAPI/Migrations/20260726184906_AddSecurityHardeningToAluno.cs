using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrilhaDaMultiplicacaoAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityHardeningToAluno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueadoAte",
                table: "Alunos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecurityStamp",
                table: "Alunos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "TentativasLoginFalhas",
                table: "Alunos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // A coluna nova entra com Guid.Empty pra linhas existentes (padrão de CLR, não dá pra
            // traduzir "Guid.NewGuid()" pra um default de coluna SQL) — troca por um GUID de verdade,
            // já que o valor é usado pra invalidar tokens e não deveria ser previsível.
            migrationBuilder.Sql("UPDATE Alunos SET SecurityStamp = NEWID();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloqueadoAte",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "TentativasLoginFalhas",
                table: "Alunos");
        }
    }
}
