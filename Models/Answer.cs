using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class Answer
    {
        public int AnswerId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(255)]
        public string AnswerText { get; set; }

        [Required]
        public bool IsCorrect { get; set; }

        public int? AnswerMediaId { get; set; }

        public Question Question { get; set; }
        public Media Media { get; set; }
    }
}
