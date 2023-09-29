using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Quizzler_Backend.Migrations
{
    /// <inheritdoc />
    public partial class flashcardLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlashcardLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WasCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FlashcardId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashcardLog_Flashcard_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcard",
                        principalColumn: "FlashcardId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardLog_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardLog_FlashcardId",
                table: "FlashcardLog",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardLog_UserId",
                table: "FlashcardLog",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardLog");
        }
    }
}
