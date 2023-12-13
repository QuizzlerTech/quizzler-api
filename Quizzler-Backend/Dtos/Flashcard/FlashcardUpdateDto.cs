namespace Quizzler_Backend.Dtos.Flashcard
{
    public class FlashcardUpdateDto
    {
        public int FlashcardId { get; set; }
        public string? QuestionText { get; set; }
        public string? AnswerText { get; set; }
        public IFormFile? QuestionImage { get; set; }
        public IFormFile? AnswerImage { get; set; }

    }
}
