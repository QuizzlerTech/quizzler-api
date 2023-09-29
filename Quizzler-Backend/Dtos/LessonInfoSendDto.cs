using Quizzler_Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class LessonInfoSendDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public DateTime DateCreated { get; set; }
        public List<string>? Tags { get; set; }
        public bool isPublic { get; set; }
    }
}
