using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Models;

namespace Quizzler_Backend.Services
{
    public class FlashcardService
    {
        private readonly GlobalService _globalService;

        public FlashcardService(GlobalService globalService)
        {
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
