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
        public string QuestionText { get; set; }

        public int? QuestionMediaId { get; set; }

        [StringLength(200)]
        public string AnswerText { get; set; }

        public int? AnswerMediaId { get; set; }

        public Lesson Lesson { get; set; }
        public Media QuestionMedia { get; set; }
        public Media AnswerMedia { get; set; }
    }
}
