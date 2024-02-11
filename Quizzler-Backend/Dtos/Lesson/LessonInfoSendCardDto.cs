using Quizzler_Backend.Dtos.User;

namespace Quizzler_Backend.Dtos.Lesson
{
    // Data Transfer Object
    public class LessonInfoSendCardDto
    {
        public int LessonId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public int FlashcardCount { get; set; }
        public UserSendDto Owner { get; set; } = null!;
        public int LikesCount { get; set; }
        public bool IsLiked { get; set; }
    }
}
