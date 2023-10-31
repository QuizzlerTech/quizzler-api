using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        [StringLength(255)]
        public string? QuestionText { get; set; }

        public int? QuestionMediaId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; } = null!;
        public virtual Media Media { get; set; } = null!;
        public virtual List<Answer> Answers { get; set; } = new List<Answer>();
    }
}
