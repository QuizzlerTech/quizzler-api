using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class Lesson
    {
        public int LessonId { get; set; }

        [Required]
        public int LessonOwner { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        [Required]
        [StringLength(40)]
        public string Title { get; set; }

        [Required]
        [StringLength(150)]
        public string Description { get; set; }

        public DateTime DateCreated { get; set; }

        public List<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
        public User User { get; set; }
    }
}

