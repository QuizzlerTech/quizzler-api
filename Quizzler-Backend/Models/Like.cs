using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class Like
    {
        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
