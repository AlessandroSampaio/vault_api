using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaultApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "licenca",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serial = table.Column<string>(type: "text", nullable: false),
                    algoritmo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_emissao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_licenca", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "licenca");
        }
    }
}
