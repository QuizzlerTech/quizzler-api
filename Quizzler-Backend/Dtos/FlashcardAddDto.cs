namespace Quizzler_Backend.Dtos
{
    public class FlashcardAddDto
    {
        public int LessonId { get; set; }
        public string? QuestionText { get; set; }
        public string? AnswerText { get; set; }
        public IFormFile? QuestionImage { get; set; }
        public IFormFile? AnswerImage { get; set; }

    }
}
