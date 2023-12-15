using Quizzler_Backend.Dtos.Flashcard;

namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class LessonSendDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsPublic { get; set; }
        public UserSendDto Owner { get; set; } = null!;
        public int LikesCount { get; set; }
        public bool IsLiked { get; set; }
        public List<string>? Tags { get; set; }
        public ICollection<FlashcardSendDto> Flashcards { get; set; } = null!;
    }
}
