using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizzler_Backend.Migrations
{
    /// <inheritdoc />
    public partial class deleteBehaviorCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_User_OwnerId",
                table: "Lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_MediaType_MediaTypeId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_User_UploaderId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_User_QuizOwner",
                table: "Quiz");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_User_OwnerId",
                table: "Lesson",
                column: "OwnerId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Media_MediaType_MediaTypeId",
                table: "Media",
                column: "MediaTypeId",
                principalTable: "MediaType",
                principalColumn: "MediaTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Media_User_UploaderId",
                table: "Media",
                column: "UploaderId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_User_QuizOwner",
                table: "Quiz",
                column: "QuizOwner",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_User_OwnerId",
                table: "Lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_MediaType_MediaTypeId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_User_UploaderId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_User_QuizOwner",
                table: "Quiz");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_User_OwnerId",
                table: "Lesson",
                column: "OwnerId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Media_MediaType_MediaTypeId",
                table: "Media",
                column: "MediaTypeId",
                principalTable: "MediaType",
                principalColumn: "MediaTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Media_User_UploaderId",
                table: "Media",
                column: "UploaderId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_User_QuizOwner",
                table: "Quiz",
                column: "QuizOwner",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
