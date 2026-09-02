using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaultApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandClienteRevendaPersonalInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nome",
                table: "revenda",
                newName: "razao_social");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "cliente",
                newName: "razao_social");

            migrationBuilder.AddColumn<string>(
                name: "bairro",
                table: "revenda",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cep",
                table: "revenda",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cidade",
                table: "revenda",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complemento",
                table: "revenda",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "criado_em",
                table: "revenda",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "revenda",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "revenda",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logradouro",
                table: "revenda",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome_fantasia",
                table: "revenda",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero",
                table: "revenda",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "responsavel",
                table: "revenda",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefone",
                table: "revenda",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "whatsapp",
                table: "revenda",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bairro",
                table: "cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cep",
                table: "cliente",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cidade",
                table: "cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complemento",
                table: "cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "criado_em",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "cliente",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "cliente",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logradouro",
                table: "cliente",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome_fantasia",
                table: "cliente",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "responsavel",
                table: "cliente",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefone",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "whatsapp",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bairro",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "cep",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "cidade",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "complemento",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "criado_em",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "email",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "logradouro",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "nome_fantasia",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "numero",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "responsavel",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "telefone",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "whatsapp",
                table: "revenda");

            migrationBuilder.DropColumn(
                name: "bairro",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "cep",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "cidade",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "complemento",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "criado_em",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "email",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "logradouro",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "nome_fantasia",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "numero",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "responsavel",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "telefone",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "whatsapp",
                table: "cliente");

            migrationBuilder.RenameColumn(
                name: "razao_social",
                table: "revenda",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "razao_social",
                table: "cliente",
                newName: "nome");
        }
    }
}
