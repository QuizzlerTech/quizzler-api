using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class Media
    {
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

        public MediaType MediaType { get; set; }
        public User User { get; set; }
    }
}
