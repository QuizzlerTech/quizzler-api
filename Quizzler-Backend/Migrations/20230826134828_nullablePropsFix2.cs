using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizzler_Backend.Migrations
{
    /// <inheritdoc />
    public partial class nullablePropsFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Media_AnswerMediaId",
                table: "Flashcard");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Media_QuestionMediaId",
                table: "Flashcard");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionMediaId",
                table: "Flashcard",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AnswerMediaId",
                table: "Flashcard",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Media_AnswerMediaId",
                table: "Flashcard",
                column: "AnswerMediaId",
                principalTable: "Media",
                principalColumn: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Media_QuestionMediaId",
                table: "Flashcard",
                column: "QuestionMediaId",
                principalTable: "Media",
                principalColumn: "MediaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Media_AnswerMediaId",
                table: "Flashcard");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Media_QuestionMediaId",
                table: "Flashcard");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionMediaId",
                table: "Flashcard",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AnswerMediaId",
                table: "Flashcard",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Media_AnswerMediaId",
                table: "Flashcard",
                column: "AnswerMediaId",
                principalTable: "Media",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Media_QuestionMediaId",
                table: "Flashcard",
                column: "QuestionMediaId",
                principalTable: "Media",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
