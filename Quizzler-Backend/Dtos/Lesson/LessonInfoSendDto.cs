namespace Quizzler_Backend.Dtos.Lesson
{
    // Data Transfer Object
    public class LessonInfoSendDto
    {
        public int LessonId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public DateTime DateCreated { get; set; }
        public List<string>? Tags { get; set; }
        public bool IsPublic { get; set; }
        public int FlashcardCount { get; set; }
        public UserSendDto Owner { get; set; } = null!;
    }
}
