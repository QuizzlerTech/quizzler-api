using Quizzler_Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class LessonAddDto
    {
        public bool IsPublic { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }
}
