namespace Quizzler_Backend.Models
{
    public class Token
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

}
