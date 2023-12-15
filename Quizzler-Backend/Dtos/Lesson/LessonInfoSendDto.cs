namespace Quizzler_Backend.Dtos.Lesson
{
    // Data Transfer Object
    public class LessonInfoSendDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageName { get; set; }
        public string? Description { get; set; }
        public int FlashcardCount { get; set; }
        public UserSendDto Owner { get; set; } = null!;
        public ICollection<string> Tags { get; set; } = null!;
        public bool IsPublic { get; set; }
        public DateTime DateCreated { get; set; }
        public int LikesCount { get; set; }
        public bool IsLiked { get; set; }
    }
}
