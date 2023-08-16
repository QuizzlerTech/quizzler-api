using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(32)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? FirstName { get; set; }

        [StringLength(20)]
        public string? LastName { get; set; }

        public DateTime DateRegistered { get; set; }
        public DateTime LastSeen { get; set; }

        public int Avatar { get; set; } = 1;
        public List<Lesson> Lesson { get; set; } = new List<Lesson>();
        public List<Media> Media { get; set; } = new List<Media>();
        public LoginInfo LoginInfo { get; set; }

    }
}
