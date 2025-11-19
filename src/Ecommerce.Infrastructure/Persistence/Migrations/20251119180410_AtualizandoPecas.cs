using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtualizandoPecas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Pecas");

            migrationBuilder.RenameColumn(
                name: "Tamanho",
                table: "Pecas",
                newName: "Categoria");

            migrationBuilder.RenameColumn(
                name: "QuantidadeEstoque",
                table: "Pecas",
                newName: "Estoque");

            migrationBuilder.AlterColumn<decimal>(
                name: "Preco",
                table: "Pecas",
                type: "decimal(18,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "Pecas",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "Pecas");

            migrationBuilder.RenameColumn(
                name: "Estoque",
                table: "Pecas",
                newName: "QuantidadeEstoque");

            migrationBuilder.RenameColumn(
                name: "Categoria",
                table: "Pecas",
                newName: "Tamanho");

            migrationBuilder.AlterColumn<decimal>(
                name: "Preco",
                table: "Pecas",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Pecas",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
