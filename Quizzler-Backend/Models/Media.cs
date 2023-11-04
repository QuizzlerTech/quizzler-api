using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class Media
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        public int MediaTypeId { get; set; }

        [Required]
        public int UploaderId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        public long FileSize { get; set; }

        public int? AnswerId { get; set; }
        public int? QuestionId { get; set; }
        public int? LessonId { get; set; }
        public int? FlashcardQuestionId { get; set; }
        public int? FlashcardAnswerId { get; set; }
        public int? QuizId { get; set; }

        [ForeignKey("MediaTypeId")]
        public virtual MediaType MediaType { get; set; } = null!;
        [ForeignKey("UploaderId")]
        public virtual User Uploader { get; set; } = null!;
        [ForeignKey("AnswerId")]
        public virtual Answer Answer { get; set; } = null!;
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;
        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;
        [ForeignKey("FlashcardQuestionId")]
        public virtual Flashcard FlashcardQuestion { get; set; } = null!;
        [ForeignKey("FlashcardAnswerId")]
        public virtual Flashcard FlashcardAnswer { get; set; } = null!;
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; } = null!;

    }
}
