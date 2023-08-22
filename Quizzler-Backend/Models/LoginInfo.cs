using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class LoginInfo
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Salt { get; set; }

        public User User { get; set; }
    }
}
