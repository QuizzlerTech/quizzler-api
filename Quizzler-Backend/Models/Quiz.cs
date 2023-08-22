using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        [Required]
        public int QuizOwner { get; set; }

        [Required]
        [StringLength(40)]
        public string Title { get; set; }

        [Required]
        [StringLength(150)]
        public string Description { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        public DateTime DateCreated { get; set; }

        public List<Question> Questions { get; set; } = new List<Question>();
        public User User { get; set; }
    }
}
