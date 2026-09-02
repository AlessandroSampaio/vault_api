using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaultApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricoPrecoCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "historico_preco_catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_valor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor_anterior = table.Column<decimal>(type: "numeric", nullable: false),
                    valor_novo = table.Column<decimal>(type: "numeric", nullable: false),
                    data_alteracao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historico_preco_catalogo", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historico_preco_catalogo");
        }
    }
}
