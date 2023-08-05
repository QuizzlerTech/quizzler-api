namespace Quizzler_Backend.Dtos
{
    // Data Transfer Object
    public class UserRegisterDto
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
