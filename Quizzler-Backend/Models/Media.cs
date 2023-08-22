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
        [StringLength(500)]
        public string Path { get; set; }

        [Required]
        public long FileSize { get; set; }

        [ForeignKey("MediaTypeId")]
        public virtual MediaType MediaType { get; set; }
        [ForeignKey("UploaderId")]
        public virtual User Uploader { get; set; }
    }
}
