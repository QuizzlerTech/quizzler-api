using Quizzler_Backend.Dtos.Lesson;

namespace Quizzler_Backend.Dtos.Search
{
    public class CombinedSearchSendDto
    {
        public List<UserSendDto> Users { get; set; } = new List<UserSendDto>();
        public List<LessonInfoSendDto> Lessons { get; set; } = new List<LessonInfoSendDto>();
    }
}
