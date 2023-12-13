namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class UserRegisterDto
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
