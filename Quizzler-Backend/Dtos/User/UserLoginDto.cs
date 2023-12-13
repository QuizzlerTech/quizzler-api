namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class UserLoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
