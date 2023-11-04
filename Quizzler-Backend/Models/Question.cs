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


        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; } = null!;
        public virtual Media? QuestionMedia { get; set; } = null!;
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
