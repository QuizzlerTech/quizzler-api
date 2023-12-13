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
        public string Title { get; set; } = null!;

        [StringLength(150)]
        public string? Description { get; set; }

        public DateTime DateCreated { get; set; }
        public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
        public virtual ICollection<LessonTag> LessonTags { get; set; } = new List<LessonTag>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; } = null!;
        public virtual Media? LessonMedia { get; set; }
    }
}

