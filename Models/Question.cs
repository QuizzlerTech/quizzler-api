using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        [StringLength(255)]
        public string QuestionText { get; set; }

        public int? QuestionMediaId { get; set; }

        public Quiz Quiz { get; set; }
        public Media Media { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }
}
