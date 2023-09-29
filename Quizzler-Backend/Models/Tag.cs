using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual List<LessonTag> LessonTags { get; set; } = new List<LessonTag>();
    }
}
