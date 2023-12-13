namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class UserSendDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime LastSeen { get; set; }
        public int? Avatar { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int LessonCount { get; set; }
    }
}
