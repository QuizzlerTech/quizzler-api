namespace Quizzler_Backend.Models
{
    public class Token
    {
        public string AccessToken { get; set; } = null!;
        public string TokenType { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

}
