using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaultApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contrato",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revenda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contrato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contrato_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_adesao_override = table.Column<decimal>(type: "numeric", nullable: true),
                    valor_mensalidade_override = table.Column<decimal>(type: "numeric", nullable: true),
                    tipo_desconto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    valor_desconto = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contrato_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_contrato_item_contrato_contrato_id",
                        column: x => x.contrato_id,
                        principalTable: "contrato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contrato_item_modulo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modulo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modulo_variante_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    valor_override = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contrato_item_modulo", x => x.id);
                    table.ForeignKey(
                        name: "fk_contrato_item_modulo_contrato_item_contrato_item_id",
                        column: x => x.contrato_item_id,
                        principalTable: "contrato_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contrato_item_unidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contrato_item_unidade", x => x.id);
                    table.ForeignKey(
                        name: "fk_contrato_item_unidade_contrato_item_contrato_item_id",
                        column: x => x.contrato_item_id,
                        principalTable: "contrato_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contrato_item_contrato_id",
                table: "contrato_item",
                column: "contrato_id");

            migrationBuilder.CreateIndex(
                name: "ix_contrato_item_modulo_contrato_item_id",
                table: "contrato_item_modulo",
                column: "contrato_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_contrato_item_unidade_contrato_item_id_tipo_unidade",
                table: "contrato_item_unidade",
                columns: new[] { "contrato_item_id", "tipo_unidade" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contrato_item_modulo");

            migrationBuilder.DropTable(
                name: "contrato_item_unidade");

            migrationBuilder.DropTable(
                name: "contrato_item");

            migrationBuilder.DropTable(
                name: "contrato");
        }
    }
}
