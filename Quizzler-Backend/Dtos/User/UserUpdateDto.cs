﻿namespace Quizzler_Backend.Dtos.User
{
    public class UserUpdateDto
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string CurrentPassword { get; set; } = null!;

    }
}
