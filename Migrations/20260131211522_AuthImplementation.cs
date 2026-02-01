using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RememberAll.src.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuthImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListAccesss_TodoLists_ListId",
                table: "ListAccesss");

            migrationBuilder.DropForeignKey(
                name: "FK_ListAccesss_Users_UserId",
                table: "ListAccesss");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ListAccesss",
                table: "ListAccesss");

            migrationBuilder.RenameTable(
                name: "ListAccesss",
                newName: "ListAccess");

            migrationBuilder.RenameIndex(
                name: "IX_ListAccesss_UserId",
                table: "ListAccess",
                newName: "IX_ListAccess_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ListAccesss_ListId_UserId",
                table: "ListAccess",
                newName: "IX_ListAccess_ListId_UserId");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListAccess",
                table: "ListAccess",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ListAccess_TodoLists_ListId",
                table: "ListAccess",
                column: "ListId",
                principalTable: "TodoLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ListAccess_Users_UserId",
                table: "ListAccess",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListAccess_TodoLists_ListId",
                table: "ListAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_ListAccess_Users_UserId",
                table: "ListAccess");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ListAccess",
                table: "ListAccess");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "ListAccess",
                newName: "ListAccesss");

            migrationBuilder.RenameIndex(
                name: "IX_ListAccess_UserId",
                table: "ListAccesss",
                newName: "IX_ListAccesss_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ListAccess_ListId_UserId",
                table: "ListAccesss",
                newName: "IX_ListAccesss_ListId_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListAccesss",
                table: "ListAccesss",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ListAccesss_TodoLists_ListId",
                table: "ListAccesss",
                column: "ListId",
                principalTable: "TodoLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ListAccesss_Users_UserId",
                table: "ListAccesss",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
