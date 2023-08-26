using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class Flashcard
    {
        [Key]
        public int FlashcardId { get; set; }

        [Required]
        public int LessonId { get; set; }

        public DateTime DateCreated { get; set; }

        [StringLength(200)]
        public string? QuestionText { get; set; }

        public int? QuestionMediaId { get; set; }

        [StringLength(200)]
        public string? AnswerText { get; set; }

        public int? AnswerMediaId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; }
        [ForeignKey("QuestionMediaId")]
        public virtual Media? QuestionMedia { get; set; }
        [ForeignKey("AnswerMediaId")]
        public virtual Media? AnswerMedia { get; set; }
    }
}
