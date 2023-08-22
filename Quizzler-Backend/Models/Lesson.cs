using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class Lesson
    {
        [Key]
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
        public int? LessonMediaId { get; set; } = null;

        public List<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
        [ForeignKey("LessonOwner")]
        public User User { get; set; }
        [ForeignKey("LessonMediaId")]
        public Media Media { get; set; }
    }
}

