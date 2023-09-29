using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizzler_Backend.Models
{
    public class FlashcardLog
    {
        [Key]
        public int Id { get; set; }
        public bool WasCorrect { get; set; }
        public int FlashcardId { get; set; }
        public int? UserId { get; set; }
        public DateTime Date { get; set; }
        [ForeignKey("FlashcardId")]
        public virtual Flashcard Flashcard { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
