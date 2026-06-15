using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSaltEdgeAndAddEnableBankingSessionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaltEdgeTransactions");

            migrationBuilder.DropTable(
                name: "SaltEdgeAccounts");

            migrationBuilder.DropTable(
                name: "SaltEdgeConnections");

            migrationBuilder.DropTable(
                name: "SaltEdgeCustomers");

            migrationBuilder.AddColumn<Guid>(
                name: "EnableBankingSessionId",
                table: "BankConsent",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableBankingSessionId",
                table: "BankConsent");

            migrationBuilder.CreateTable(
                name: "SaltEdgeCustomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<string>(type: "text", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaltEdgeCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaltEdgeCustomers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaltEdgeConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaltEdgeCustomerId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BankImgUrl = table.Column<string>(type: "text", nullable: false),
                    ConnectionId = table.Column<string>(type: "text", nullable: true),
                    CountryCode = table.Column<string>(type: "text", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorClass = table.Column<string>(type: "text", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
                    LastStage = table.Column<string>(type: "text", nullable: true),
                    LastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    LocalSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderCode = table.Column<string>(type: "text", nullable: false),
                    ProviderName = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    SyncStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaltEdgeConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaltEdgeConnections_SaltEdgeCustomers_SaltEdgeCustomerId",
                        column: x => x.SaltEdgeCustomerId,
                        principalTable: "SaltEdgeCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaltEdgeConnections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaltEdgeAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConnectionId = table.Column<int>(type: "integer", nullable: true),
                    AccountId = table.Column<string>(type: "text", nullable: false),
                    ConnectionDbId = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    HolderName = table.Column<string>(type: "text", nullable: false),
                    Iban = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaltEdgeAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaltEdgeAccounts_SaltEdgeConnections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "SaltEdgeConnections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaltEdgeTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaltEdgeAccountId = table.Column<int>(type: "integer", nullable: false),
                    SpendingId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Categorized = table.Column<bool>(type: "boolean", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ExternalTransactionId = table.Column<string>(type: "text", nullable: false),
                    MerchantCode = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaltEdgeTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaltEdgeTransactions_SaltEdgeAccounts_SaltEdgeAccountId",
                        column: x => x.SaltEdgeAccountId,
                        principalTable: "SaltEdgeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaltEdgeTransactions_Spendings_SpendingId",
                        column: x => x.SpendingId,
                        principalTable: "Spendings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeAccounts_AccountId",
                table: "SaltEdgeAccounts",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeAccounts_ConnectionId",
                table: "SaltEdgeAccounts",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeConnections_ConnectionId",
                table: "SaltEdgeConnections",
                column: "ConnectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeConnections_LocalSessionId",
                table: "SaltEdgeConnections",
                column: "LocalSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeConnections_SaltEdgeCustomerId",
                table: "SaltEdgeConnections",
                column: "SaltEdgeCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeConnections_UserId",
                table: "SaltEdgeConnections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeCustomers_CustomerId",
                table: "SaltEdgeCustomers",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeCustomers_UserId",
                table: "SaltEdgeCustomers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeTransactions_SaltEdgeAccountId",
                table: "SaltEdgeTransactions",
                column: "SaltEdgeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeTransactions_SpendingId",
                table: "SaltEdgeTransactions",
                column: "SpendingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaltEdgeTransactions_TransactionId",
                table: "SaltEdgeTransactions",
                column: "TransactionId",
                unique: true);
        }
    }
}
