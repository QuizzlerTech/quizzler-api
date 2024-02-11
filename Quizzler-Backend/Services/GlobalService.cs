using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.EntityFrameworkCore;
using MlkPwgen;
using Quizzler_Backend.Data;
using Quizzler_Backend.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;
using System.Text;


namespace Quizzler_Backend.Services
{
    public class GlobalService(QuizzlerDbContext context)
    {
        private readonly QuizzlerDbContext _context = context;

        public int? GetUserIdFromClaims(ClaimsPrincipal user)
        {
            if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return null;
            }
            return userId;
        }
        public async Task<Media> SaveImage(IFormFile file, string fileName, int uploaderId)
        {
            var imageMediaType = await _context.MediaType.FirstOrDefaultAsync(u => u.TypeName == "Image") ?? throw new InvalidOperationException("No media type found for 'Image'.");
            var imagesPath = Environment.GetEnvironmentVariable("ImagesPath") ?? throw new InvalidOperationException("ImagesPath configuration value is null or empty.");
            var outputPath = Path.Combine(imagesPath, fileName);

            await using (var inputStream = file.OpenReadStream())
            {
                using var image = await Image.LoadAsync(inputStream);
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 600),
                    Mode = ResizeMode.Max
                }));

                await image.SaveAsync(outputPath, new JpegEncoder { Quality = 80 });
            }
            Media media = new()
            {
                MediaTypeId = imageMediaType.MediaTypeId,
                UploaderId = uploaderId,
                Name = fileName,
                FileSize = file.Length
            };

            return media;
        }
        public async Task<bool> DeleteImage(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            var imagesPath = Environment.GetEnvironmentVariable("ImagesPath") ?? throw new InvalidOperationException("The environment variable 'ImagesPath' is not set.");

            var fullImagePath = Path.Combine(imagesPath, Path.GetFileName(fileName));
            if (!File.Exists(fullImagePath)) throw new FileNotFoundException("The specified file does not exist.", fullImagePath);

            await Task.Run(() => File.Delete(fullImagePath));
            return true;

        }
        public async Task<bool> IsImageRightSize(IFormFile file)
        {
            var imageMediaType = await _context.MediaType.FirstOrDefaultAsync(u => u.TypeName == "Image") ?? throw new InvalidOperationException("No media type found for 'Image'.");
            return file.Length > 0 && file.Length < imageMediaType.MaxSize;
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

            using SecureArray<byte> hash = argon2.Hash();
            return Convert.ToBase64String(hash.Buffer); // used Isopoh.Cryptography.Argon2
        }
        public string CreateSalt()
        {
            return PasswordGenerator.Generate(length: 16, allowed: Sets.Alphanumerics); // used MlkPwgen
        }
    }
}
