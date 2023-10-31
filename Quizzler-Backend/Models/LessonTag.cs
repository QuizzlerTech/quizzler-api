using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class LessonTag
    {
        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;

        public int TagId { get; set; }
        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; } = null!;
    }
}
