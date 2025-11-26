using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedMonthlyBudgetAddedBudgetPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetCategories_MonthlyBudgets_MonthlyBudgetId",
                table: "BudgetCategories");

            migrationBuilder.DropTable(
                name: "MonthlyBudgets");

            migrationBuilder.RenameColumn(
                name: "MonthlyBudgetId",
                table: "BudgetCategories",
                newName: "BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetCategories_MonthlyBudgetId",
                table: "BudgetCategories",
                newName: "IX_BudgetCategories_BudgetId");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Spendings",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "BudgetPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BudgetId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetPeriods_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetPeriods_BudgetId",
                table: "BudgetPeriods",
                column: "BudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetCategories_Budgets_BudgetId",
                table: "BudgetCategories",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetCategories_Budgets_BudgetId",
                table: "BudgetCategories");

            migrationBuilder.DropTable(
                name: "BudgetPeriods");

            migrationBuilder.RenameColumn(
                name: "BudgetId",
                table: "BudgetCategories",
                newName: "MonthlyBudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetCategories_BudgetId",
                table: "BudgetCategories",
                newName: "IX_BudgetCategories_MonthlyBudgetId");

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "Spendings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.CreateTable(
                name: "MonthlyBudgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BudgetId = table.Column<int>(type: "integer", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyBudgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyBudgets_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyBudgets_BudgetId",
                table: "MonthlyBudgets",
                column: "BudgetId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetCategories_MonthlyBudgets_MonthlyBudgetId",
                table: "BudgetCategories",
                column: "MonthlyBudgetId",
                principalTable: "MonthlyBudgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
