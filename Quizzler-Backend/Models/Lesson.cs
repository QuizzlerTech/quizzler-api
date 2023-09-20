using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required]
        public int OwnerId { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        [Required]
        [StringLength(40)]
        public string Title { get; set; }

        [StringLength(150)]
        public string? Description { get; set; }

        public DateTime DateCreated { get; set; }
        public int? LessonMediaId { get; set; } = null;

        public virtual List<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; }
        [ForeignKey("LessonMediaId")]
        public virtual Media? Media { get; set; }
    }
}

