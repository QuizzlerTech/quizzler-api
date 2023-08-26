using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MlkPwgen;
using Quizzler_Backend.Controllers;
using Quizzler_Backend.Models;

using SixLabors.ImageSharp.Formats.Jpeg;
using System.Text;


namespace Quizzler_Backend.Services
{
    public class GlobalService
    {
        private readonly QuizzlerDbContext _context;
        private readonly ILogger<LessonController> _logger;

        public GlobalService(QuizzlerDbContext context, ILogger<LessonController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Media> SaveImage(IFormFile file, string fileName, int uploaderId)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }
            string outputPath = Environment.GetEnvironmentVariable("ImagesPath") + fileName;
            try
            {
                using (var inputStream = file.OpenReadStream())
                {
                    using (var image = Image.Load(inputStream))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(800, 600),
                            Mode = ResizeMode.Max
                        }));

                        await image.SaveAsync(outputPath, new JpegEncoder { Quality = 70 });
                    }
                }

                Media media = new Media();
                media.MediaTypeId = (await _context.MediaType.FirstOrDefaultAsync(u => u.TypeName == "Image")).MediaTypeId;
                media.UploaderId = uploaderId;
                media.Path = outputPath;
                media.FileSize = file.Length;

                return media;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                return null;
            }
        }
        // Hash a password using Argon2
        public string HashPassword(string password, string salt)
        {
            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                MemoryCost = 32768,
                Threads = Environment.ProcessorCount,
                Password = Encoding.UTF8.GetBytes(password),
                Salt = Encoding.UTF8.GetBytes(salt),
                HashLength = 60
            };

            var argon2 = new Argon2(config);

            using (SecureArray<byte> hash = argon2.Hash())
            {
                return Convert.ToBase64String(hash.Buffer); // used Isopoh.Cryptography.Argon2
            }
        }
        // Create a new salt for password hashing
        public string CreateSalt()
        {
            return PasswordGenerator.Generate(length: 16, allowed: Sets.Alphanumerics); // used MlkPwgen
        }
    }
}
