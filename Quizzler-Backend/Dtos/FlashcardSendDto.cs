namespace Quizzler_Backend.Dtos
{
    public class FlashcardSendDto
    {
        public int FlashcardId { get; set; }
        public DateTime DateCreated { get; set; }
        public string? QuestionText { get; set; }
        public string? AnswerText { get; set; }
        public string? QuestionImagePath { get; set; }
        public string? AnswerImagePath { get; set; }

    }
}
