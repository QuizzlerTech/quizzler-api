﻿namespace Quizzler_Backend.Dtos.Lesson
{
    // Data Transfer Object
    public class LessonUpdateDto
    {
        public int LessonId { get; set; }
        public bool? IsPublic { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string>? TagNames { get; set; }
        public IFormFile? Image { get; set; }
    }
}
