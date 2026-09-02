using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaultApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameIdentityTablesSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_claims_asp_net_users_user_id",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_logins_asp_net_users_user_id",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_roles_asp_net_users_user_id",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_tokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_users",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_roles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_logins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_claims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_roles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_role_claims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "usuario_tokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "usuarios");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "usuario_roles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "usuario_logins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "usuario_claims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "role_claims");

            migrationBuilder.RenameIndex(
                name: "UserNameIndex",
                table: "usuarios",
                newName: "ix_usuarios_normalized_user_name");

            migrationBuilder.RenameIndex(
                name: "EmailIndex",
                table: "usuarios",
                newName: "ix_usuarios_normalized_email");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "usuario_roles",
                newName: "ix_usuario_roles_role_id");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "usuario_logins",
                newName: "ix_usuario_logins_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "usuario_claims",
                newName: "ix_usuario_claims_user_id");

            migrationBuilder.RenameIndex(
                name: "RoleNameIndex",
                table: "roles",
                newName: "ix_roles_normalized_name");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "role_claims",
                newName: "ix_role_claims_role_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_usuario_tokens",
                table: "usuario_tokens",
                columns: new[] { "user_id", "login_provider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_usuarios",
                table: "usuarios",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_usuario_roles",
                table: "usuario_roles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_usuario_logins",
                table: "usuario_logins",
                columns: new[] { "login_provider", "provider_key" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_usuario_claims",
                table: "usuario_claims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_roles",
                table: "roles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_role_claims",
                table: "role_claims",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_role_claims_roles_role_id",
                table: "role_claims",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_usuario_claims_usuarios_user_id",
                table: "usuario_claims",
                column: "user_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_usuario_logins_usuarios_user_id",
                table: "usuario_logins",
                column: "user_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_usuario_roles_roles_role_id",
                table: "usuario_roles",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_usuario_roles_usuarios_user_id",
                table: "usuario_roles",
                column: "user_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_usuario_tokens_usuarios_user_id",
                table: "usuario_tokens",
                column: "user_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_claims_roles_role_id",
                table: "role_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_usuario_claims_usuarios_user_id",
                table: "usuario_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_usuario_logins_usuarios_user_id",
                table: "usuario_logins");

            migrationBuilder.DropForeignKey(
                name: "fk_usuario_roles_roles_role_id",
                table: "usuario_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_usuario_roles_usuarios_user_id",
                table: "usuario_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_usuario_tokens_usuarios_user_id",
                table: "usuario_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_usuarios",
                table: "usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "pk_usuario_tokens",
                table: "usuario_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_usuario_roles",
                table: "usuario_roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_usuario_logins",
                table: "usuario_logins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_usuario_claims",
                table: "usuario_claims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_role_claims",
                table: "role_claims");

            migrationBuilder.RenameTable(
                name: "usuarios",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "usuario_tokens",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "usuario_roles",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "usuario_logins",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "usuario_claims",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "role_claims",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameIndex(
                name: "ix_usuarios_normalized_user_name",
                table: "AspNetUsers",
                newName: "UserNameIndex");

            migrationBuilder.RenameIndex(
                name: "ix_usuarios_normalized_email",
                table: "AspNetUsers",
                newName: "EmailIndex");

            migrationBuilder.RenameIndex(
                name: "ix_usuario_roles_role_id",
                table: "AspNetUserRoles",
                newName: "ix_asp_net_user_roles_role_id");

            migrationBuilder.RenameIndex(
                name: "ix_usuario_logins_user_id",
                table: "AspNetUserLogins",
                newName: "ix_asp_net_user_logins_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_usuario_claims_user_id",
                table: "AspNetUserClaims",
                newName: "ix_asp_net_user_claims_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_roles_normalized_name",
                table: "AspNetRoles",
                newName: "RoleNameIndex");

            migrationBuilder.RenameIndex(
                name: "ix_role_claims_role_id",
                table: "AspNetRoleClaims",
                newName: "ix_asp_net_role_claims_role_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_users",
                table: "AspNetUsers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_tokens",
                table: "AspNetUserTokens",
                columns: new[] { "user_id", "login_provider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_roles",
                table: "AspNetUserRoles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_logins",
                table: "AspNetUserLogins",
                columns: new[] { "login_provider", "provider_key" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_claims",
                table: "AspNetUserClaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_roles",
                table: "AspNetRoles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_role_claims",
                table: "AspNetRoleClaims",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                table: "AspNetRoleClaims",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_claims_asp_net_users_user_id",
                table: "AspNetUserClaims",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_logins_asp_net_users_user_id",
                table: "AspNetUserLogins",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_roles_asp_net_users_user_id",
                table: "AspNetUserRoles",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                table: "AspNetUserTokens",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
