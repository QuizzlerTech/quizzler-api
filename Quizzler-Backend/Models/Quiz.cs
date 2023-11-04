using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        [Required]
        public int QuizOwner { get; set; }

        [Required]
        [StringLength(40)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string? Description { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        public DateTime DateCreated { get; set; }

        public virtual List<Question> Questions { get; set; } = new List<Question>();
        [ForeignKey("QuizOwner")]
        public virtual User Owner { get; set; } = null!;
        public virtual Media? QuizMedia { get; set; }
    }
}
