using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;
using System.Text.RegularExpressions;

namespace Quizzler_Backend.Controllers.Services
{
    public class LessonService
    {        
        // Field variables
        private readonly GlobalService _globalService;
        private readonly QuizzlerDbContext _context;

        // Constructor
        public LessonService(QuizzlerDbContext context, GlobalService globalService)
        {
            _context = context;
            _globalService = globalService;
        }

        // Create a new lesson
        public Lesson CreateLesson(LessonAddDto lessonAddDto, int ownerId, User user)
        {
            if (lessonAddDto == null) throw new ArgumentNullException(nameof(lessonAddDto));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var lesson = new Lesson
            {
                OwnerId = ownerId,
                IsPublic = lessonAddDto.IsPublic,
                Title = lessonAddDto.Title,
                Description = lessonAddDto.Description,
                DateCreated = DateTime.UtcNow,
                Owner = user,
            };
            return lesson;
        }


        // Check if the lesson title exists for this user
        public bool TitleExists(string title, User user)
        {
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
            return (description == null) || (description.Length <= 150);
        }
        public string MakeAlphaNumerical(string text)
        {
            return Regex.Replace(text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
        public string GenerateImageName(string title)
        {
            return MakeAlphaNumerical(title) + _globalService.CreateSalt() + ".jpeg";
        }

        // Add Tag
        public async Task<Tag> AddTag(string tagName)
        {
            var tag = new Tag { Name = tagName.ToLower() };
            await _context.Tag.AddAsync(tag);
            return tag;
        }

        // Add LessonTag
        public async Task AddLessonTag(string tagName, Lesson lesson)
        {
            var tag = await _context.Tag.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null) tag = await AddTag(tagName);
            if (lesson.LessonTags.Any(t => t.Tag.Name == tagName)) return; 
            lesson.LessonTags.Add(new LessonTag { Lesson = lesson, Tag = tag });
        }
    }
}
