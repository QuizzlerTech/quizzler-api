using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Quizzler_Backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(32)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        public string? FirstName { get; set; }

        [StringLength(20)]
        public string? LastName { get; set; }

        public DateTime DateRegistered { get; set; }
        public DateTime LastSeen { get; set; }

        public int? Avatar { get; set; }
        [JsonIgnore]
        public virtual ICollection<Lesson> Lesson { get; set; } = new List<Lesson>();
        [JsonIgnore]
        public virtual ICollection<Media> UserMedia { get; set; } = new List<Media>();
        [JsonIgnore]
        public virtual LoginInfo LoginInfo { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<FlashcardLog> FlashcardLog { get; set; } = new List<FlashcardLog>();

    }
}
