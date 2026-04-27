using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingsPots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavingsPots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsPots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsPots_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingsPots_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppUserSavingsPot",
                columns: table => new
                {
                    SavingsPotsId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserSavingsPot", x => new { x.SavingsPotsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_AppUserSavingsPot_SavingsPots_SavingsPotsId",
                        column: x => x.SavingsPotsId,
                        principalTable: "SavingsPots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserSavingsPot_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingsContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SavingsPotId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    AddedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsContributions_SavingsPots_SavingsPotId",
                        column: x => x.SavingsPotId,
                        principalTable: "SavingsPots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingsContributions_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SavingsPotInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SavingsPotId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<int>(type: "integer", nullable: false),
                    ReceiverId = table.Column<int>(type: "integer", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now()) + INTERVAL '2 days'"),
                    Accepted = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsPotInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsPotInvites_SavingsPots_SavingsPotId",
                        column: x => x.SavingsPotId,
                        principalTable: "SavingsPots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingsPotInvites_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavingsPotInvites_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserSavingsPot_UsersId",
                table: "AppUserSavingsPot",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsContributions_AddedByUserId",
                table: "SavingsContributions",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsContributions_SavingsPotId",
                table: "SavingsContributions",
                column: "SavingsPotId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsPotInvites_ReceiverId",
                table: "SavingsPotInvites",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsPotInvites_SavingsPotId",
                table: "SavingsPotInvites",
                column: "SavingsPotId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsPotInvites_SenderId",
                table: "SavingsPotInvites",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsPots_CreatedByUserId",
                table: "SavingsPots",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsPots_CurrencyId",
                table: "SavingsPots",
                column: "CurrencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserSavingsPot");

            migrationBuilder.DropTable(
                name: "SavingsContributions");

            migrationBuilder.DropTable(
                name: "SavingsPotInvites");

            migrationBuilder.DropTable(
                name: "SavingsPots");
        }
    }
}
