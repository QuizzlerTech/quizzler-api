namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class LessonSendDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsPublic { get; set; }
        public List<string>? Tags { get; set; }
        public ICollection<FlashcardSendDto> Flashcards { get; set; } = null!;
    }
}
