using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.EntityFrameworkCore;
using MlkPwgen;
using Quizzler_Backend.Data;
using Quizzler_Backend.Models;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Text;


namespace Quizzler_Backend.Services
{
    public class GlobalService
    {
        private readonly QuizzlerDbContext _context;

        public GlobalService(QuizzlerDbContext context)
        {
            _context = context;
        }

        public async Task<Media> SaveImage(IFormFile file, string fileName, int uploaderId)
        {
            var imageMediaType = await _context.MediaType.FirstOrDefaultAsync(u => u.TypeName == "Image") ?? throw new InvalidOperationException("No media type found for 'Image'.");
            var imagesPath = Environment.GetEnvironmentVariable("ImagesPath") ?? throw new InvalidOperationException("ImagesPath configuration value is null or empty.");
            string outputPath = Path.Combine(imagesPath, fileName);
            try
            {
                using (var inputStream = file.OpenReadStream())
                {
                    using var image = Image.Load(inputStream);
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
            catch (IOException ex)
            {
                throw ex;
            }
        }
        public async Task<bool> DeleteImage(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            string imagesPath = Environment.GetEnvironmentVariable("ImagesPath") ?? throw new InvalidOperationException("The environment variable 'ImagesPath' is not set.");

            var fullImagePath = Path.Combine(imagesPath, Path.GetFileName(fileName));
            if (!File.Exists(fullImagePath)) throw new FileNotFoundException("The specified file does not exist.", fullImagePath);
            try
            {
                await Task.Run(() => File.Delete(fullImagePath));
                return true;
            }
            catch (IOException ex)
            {
                throw ex;
            }
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
