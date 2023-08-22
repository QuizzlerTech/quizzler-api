using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class MediaType
    {
        [Key]
        public int MediaTypeId { get; set; }

        [Required]
        [StringLength(10)]
        public string Extension { get; set; }

        [Required]
        public string TypeName { get; set; }

        [Required]
        public long MaxSize { get; set; }

    }
}
