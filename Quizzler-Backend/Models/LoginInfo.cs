using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class LoginInfo
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string Salt { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
