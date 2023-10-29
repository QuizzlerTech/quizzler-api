﻿using Quizzler_Backend.Controllers;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.Reflection.Metadata.Ecma335;

namespace Quizzler_Backend.Services
{
    public class FlashcardService
    {
        private readonly QuizzlerDbContext _context;
        private readonly GlobalService _globalService;

        public FlashcardService(QuizzlerDbContext context, GlobalService globalService)
        {
            _context = context;
            _globalService = globalService;
        }
        public Flashcard CreateNewFlashcard(FlashcardAddDto flashcardAddDto)
        {
            var flashcard = new Flashcard
            {
                LessonId = flashcardAddDto.LessonId,
                QuestionText = flashcardAddDto.QuestionText,
                AnswerText = flashcardAddDto.AnswerText,
                DateCreated = DateTime.UtcNow,
            };
            return flashcard;
        }
        public string GenerateImageName()
        {
            return _globalService.CreateSalt() + ".jpeg";
        }
        public bool IsContentMissing(Flashcard flashcard)
        {
            return (flashcard.QuestionText == null && flashcard.QuestionMedia == null) ||
                   (flashcard.QuestionText == "" && flashcard.QuestionMedia == null) ||
                   (flashcard.AnswerText == null && flashcard.AnswerMedia == null) ||
                   (flashcard.AnswerText == "" && flashcard.AnswerMedia == null);
        }
    }
}
