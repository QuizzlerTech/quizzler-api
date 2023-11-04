namespace Quizzler_Backend.Dtos
{
    public class FlashcardSendDto
    {
        public int FlashcardId { get; set; }
        public DateTime DateCreated { get; set; }
        public string? QuestionText { get; set; }
        public string? AnswerText { get; set; }
        public string? QuestionImageName { get; set; }
        public string? AnswerImageName { get; set; }

    }
}
