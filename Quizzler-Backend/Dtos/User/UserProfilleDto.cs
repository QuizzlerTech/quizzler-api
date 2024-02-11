namespace Quizzler_Backend.Dtos.User
{
    // Data Transfer Object
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime LastSeen { get; set; }
        public int? Avatar { get; set; }
    }
}
