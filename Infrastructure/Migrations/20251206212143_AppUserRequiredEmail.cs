using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AppUserRequiredEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetInvite_Budgets_BudgetId",
                table: "BudgetInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetInvite_Users_ReceiverId",
                table: "BudgetInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetInvite_Users_SenderId",
                table: "BudgetInvite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BudgetInvite",
                table: "BudgetInvite");

            migrationBuilder.RenameTable(
                name: "BudgetInvite",
                newName: "BudgetInvites");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetInvite_SenderId",
                table: "BudgetInvites",
                newName: "IX_BudgetInvites_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetInvite_ReceiverId",
                table: "BudgetInvites",
                newName: "IX_BudgetInvites_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetInvite_BudgetId",
                table: "BudgetInvites",
                newName: "IX_BudgetInvites_BudgetId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BudgetInvites",
                table: "BudgetInvites",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetInvites_Budgets_BudgetId",
                table: "BudgetInvites",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetInvites_Users_ReceiverId",
                table: "BudgetInvites",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetInvites_Users_SenderId",
                table: "BudgetInvites",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetInvites_Budgets_BudgetId",
                table: "BudgetInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetInvites_Users_ReceiverId",
                table: "BudgetInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetInvites_Users_SenderId",
                table: "BudgetInvites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BudgetInvites",
                table: "BudgetInvites");

            migrationBuilder.RenameTable(
                name: "BudgetInvites",
                newName: "BudgetInvite");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetInvites_SenderId",
                table: "BudgetInvite",
                newName: "IX_BudgetInvite_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetInvites_ReceiverId",
                table: "BudgetInvite",
                newName: "IX_BudgetInvite_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetInvites_BudgetId",
                table: "BudgetInvite",
                newName: "IX_BudgetInvite_BudgetId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BudgetInvite",
                table: "BudgetInvite",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetInvite_Budgets_BudgetId",
                table: "BudgetInvite",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetInvite_Users_ReceiverId",
                table: "BudgetInvite",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetInvite_Users_SenderId",
                table: "BudgetInvite",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
