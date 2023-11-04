using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;
        public virtual ICollection<LessonTag> LessonTags { get; set; } = new List<LessonTag>();
    }
}
