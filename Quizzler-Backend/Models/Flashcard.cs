using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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


        [StringLength(200)]
        public string? AnswerText { get; set; }


        [ForeignKey("LessonId")]
        [JsonIgnore]
        public virtual Lesson Lesson { get; set; } = null!;
        public virtual Media? QuestionMedia { get; set; }
        public virtual Media? AnswerMedia { get; set; }
        public virtual ICollection<FlashcardLog>? FlashcardLog { get; set; }
    }
}
