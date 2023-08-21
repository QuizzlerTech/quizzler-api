using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.Reflection.Metadata;

namespace Quizzler_Backend.Controllers.Services
{
    public class LessonService
    {        
        // Field variables
        private readonly QuizzlerDbContext _context;
        private readonly IConfiguration _configuration;

        // Constructor
        public LessonService(QuizzlerDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Create a new lesson
        public async Task<Lesson> CreateLesson(LessonAddDto lessonAddDto, int ownerId, User user)
        {
            if (lessonAddDto == null) throw new ArgumentNullException(nameof(lessonAddDto));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var lesson = new Lesson
            {
                LessonOwner = ownerId,
                IsPublic = lessonAddDto.IsPublic,
                Title = lessonAddDto.Title,
                Description = lessonAddDto.Description,
                DateCreated = DateTime.UtcNow,
                User = user,
            };
            return lesson;
        }


        // Check if the lesson title exists for this user
        public bool TitleExists(string title, User user)
        {
            Console.WriteLine(user.Lesson.Count);
            if (string.IsNullOrEmpty(title)) throw new ArgumentException("Title cannot be null or empty", nameof(title));
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.Lesson.Any(l => l.Title == title);
        }

        // Check if the lesson title is in the right format
        public bool IsTitleCorrect(string title)
        {
            return (title.Length <= 40 && title.Length >= 1);
        }

        // Check if the lesson description is in the right format
        public bool IsDescriptionCorrect(string description)
        {
            return (description.Length <= 150);
        }

    }
}
