namespace Quizzler_Backend.Dtos
{
    public class LessonSearchSendDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageName { get; set; }
        public string? Description { get; set; }
        public int FlashcardCount { get; set; }
        public UserSendDto Owner { get; set; } = null!;
        public ICollection<string> Tags { get; set; } = null!;
    }
}

