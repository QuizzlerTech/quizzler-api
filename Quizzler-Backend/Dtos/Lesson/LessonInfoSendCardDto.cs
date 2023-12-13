namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class LessonInfoSendCardDto
    {
        public int LessonId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public int FlashcardCount { get; set; }
    }
}
