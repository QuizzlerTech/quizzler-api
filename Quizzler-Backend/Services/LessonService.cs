using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Dtos.Lesson;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Quizzler_Backend.Controllers.Services
{
    public class LessonService
    {
        private readonly GlobalService _globalService;
        private readonly QuizzlerDbContext _context;

        public LessonService(QuizzlerDbContext context, GlobalService globalService)
        {
            _context = context;
            _globalService = globalService;
        }
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
        public bool TitleExists(string title, User user)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException("Title cannot be null or empty", nameof(title));
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.Lesson.Any(l => l.Title == title);
        }
        public bool IsTitleCorrect(string title)
        {
            return (title.Length <= 40 && title.Length >= 1);
        }
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
        public List<FlashcardSendDto> GetOrderOfFlashcards(List<FlashcardSendDto> flashcards, List<FlashcardLog> flashcardLogs)
        {
            Dictionary<int, int> flashcardRating = new();
            flashcards.ForEach(flashcard =>
            {
                flashcardRating[flashcard.FlashcardId] = 0;
            });
            flashcardLogs.ForEach(flashcardLog =>
            {
                flashcardRating[flashcardLog.FlashcardId] += flashcardLog.WasCorrect ? -2 : 1;
            });
            flashcards.Sort((flashcard1, flashcard2) =>
                flashcardRating[flashcard2.FlashcardId].CompareTo(flashcardRating[flashcard1.FlashcardId]));
            return flashcards;
        }
        public async Task<Tag> AddTag(string tagName)
        {
            PropertyInfo? propertyInfo = typeof(Tag).GetProperty("Name");
            var stringLengthAttribute = propertyInfo?.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttribute != null && tagName.Length > stringLengthAttribute.MaximumLength)
            {
                throw new ArgumentException("Tag name is too big");
            }
            var tag = new Tag { Name = tagName.ToLower() };
            await _context.Tag.AddAsync(tag);
            return tag;
        }
        public async Task AddLessonTag(string tagName, Lesson lesson)
        {
            var tag = await _context.Tag.FirstOrDefaultAsync(t => t.Name == tagName) ?? await AddTag(tagName);
            if (lesson.LessonTags.Any(t => t.Tag.Name == tagName)) return;
            lesson.LessonTags.Add(new LessonTag { Lesson = lesson, Tag = tag });
        }
    }
}
