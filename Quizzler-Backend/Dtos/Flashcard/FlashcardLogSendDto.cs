namespace Quizzler_Backend.Dtos.Flashcard
{
    public class FlashcardLogSendDto
    {
        public int FlashcardId { get; set; }
        public bool WasCorrect { get; set; }
        public DateTime Date { get; set; }
    }
}
