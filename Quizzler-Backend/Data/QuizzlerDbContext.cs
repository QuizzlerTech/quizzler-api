using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Models;

namespace Quizzler_Backend.Data
{
    public class QuizzlerDbContext : DbContext
    {
        public QuizzlerDbContext(DbContextOptions<QuizzlerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).HasMaxLength(32).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(20);
                entity.Property(e => e.LastName).HasMaxLength(20);
                entity.Property(e => e.DateRegistered).IsRequired();
                entity.Property(e => e.Avatar);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Quiz Configuration
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.QuizId);
                entity.Property(e => e.QuizOwner).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(40).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(150);
                entity.Property(e => e.IsPublic).IsRequired();
                entity.Property(e => e.DateCreated).IsRequired();
                entity.HasOne(e => e.Owner)
                      .WithMany()
                      .HasForeignKey(e => e.QuizOwner)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Question Configuration
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.QuestionId);
                entity.Property(e => e.QuizId).IsRequired();
                entity.Property(e => e.QuestionText).HasMaxLength(255);

                entity.HasOne(q => q.Quiz)
                      .WithMany(q => q.Questions)
                      .HasForeignKey(q => q.QuizId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // MediaType Configuration
            modelBuilder.Entity<MediaType>(entity =>
            {
                entity.HasKey(e => e.MediaTypeId);
                entity.Property(e => e.Extension).HasMaxLength(10).IsRequired();
                entity.Property(e => e.TypeName).IsRequired();
                entity.Property(e => e.MaxSize).IsRequired();
            });

            // Media Configuration
            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasKey(e => e.MediaId);
                entity.Property(e => e.MediaTypeId).IsRequired();
                entity.Property(e => e.UploaderId).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.AnswerId).IsRequired(false);
                entity.Property(e => e.QuestionId).IsRequired(false);
                entity.Property(e => e.LessonId).IsRequired(false);
                entity.Property(e => e.FlashcardQuestionId).IsRequired(false);
                entity.Property(e => e.FlashcardAnswerId).IsRequired(false);
                entity.Property(e => e.QuizId).IsRequired(false);

                entity.HasOne(e => e.MediaType)
                      .WithMany()
                      .HasForeignKey(e => e.MediaTypeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Uploader)
                      .WithMany(u => u.UserMedia)
                      .HasForeignKey(e => e.UploaderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Answer)
                      .WithOne(a => a.AnswerMedia)
                      .HasForeignKey<Media>(m => m.AnswerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Question)
                      .WithOne(q => q.QuestionMedia)
                      .HasForeignKey<Media>(m => m.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Lesson)
                      .WithOne(l => l.LessonMedia)
                      .HasForeignKey<Media>(m => m.LessonId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.FlashcardQuestion)
                      .WithOne(f => f.QuestionMedia)
                      .HasForeignKey<Media>(m => m.FlashcardQuestionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.FlashcardAnswer)
                      .WithOne(f => f.AnswerMedia)
                      .HasForeignKey<Media>(m => m.FlashcardAnswerId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(m => m.Quiz)
                      .WithOne(f => f.QuizMedia)
                      .HasForeignKey<Media>(m => m.QuizId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // LoginInfo Configuration
            modelBuilder.Entity<LoginInfo>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.PasswordHash).HasMaxLength(80);
                entity.Property(e => e.Salt).HasMaxLength(128).IsRequired();
            });


            // Lesson Configuration
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasKey(e => e.LessonId);
                entity.Property(e => e.OwnerId).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(40).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(150);
                entity.Property(e => e.DateCreated).IsRequired();
                entity.Property(e => e.IsPublic).IsRequired();

                entity.HasOne(l => l.Owner)
                      .WithMany(u => u.Lesson)
                      .HasForeignKey(e => e.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Flashcard Configuration
            modelBuilder.Entity<Flashcard>(entity =>
            {
                entity.HasKey(e => e.FlashcardId);
                entity.Property(e => e.LessonId).IsRequired();
                entity.Property(e => e.DateCreated).IsRequired();
                entity.Property(e => e.QuestionText).HasMaxLength(200);
                entity.Property(e => e.AnswerText).HasMaxLength(200);

                entity.HasOne(f => f.Lesson)
                      .WithMany(l => l.Flashcards)
                      .HasForeignKey(f => f.LessonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Answer Configuration
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.HasKey(e => e.AnswerId);
                entity.Property(e => e.QuestionId).IsRequired();
                entity.Property(e => e.AnswerText).HasMaxLength(255);
                entity.Property(e => e.IsCorrect).IsRequired();

                entity.HasOne(a => a.Question)
                      .WithMany(q => q.Answers)
                      .HasForeignKey(a => a.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Tag Configuration
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasMany(t => t.LessonTags)
                      .WithOne(lt => lt.Tag)
                      .HasForeignKey(lt => lt.TagId);
            });

            // LessonTag Configuration
            modelBuilder.Entity<LessonTag>().HasKey(lt => new { lt.LessonId, lt.TagId });

            // FlashcardLog Configuration
            modelBuilder.Entity<FlashcardLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(fl => fl.Flashcard)
                      .WithMany(f => f.FlashcardLog)
                      .HasForeignKey(f => f.FlashcardId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(l => l.Lesson)
                      .WithMany()
                      .HasForeignKey(f => f.LessonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Like Configuration
            modelBuilder.Entity<Like>().HasKey(f => new { f.LessonId, f.UserId });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var DB_CONNECTION_STRING = Environment.GetEnvironmentVariable("DbConnection") ?? throw new InvalidOperationException("DbConnection env variable is null or empty.");
            optionsBuilder.UseMySQL(DB_CONNECTION_STRING);
        }

        public DbSet<User> User { get; set; }
        public DbSet<LoginInfo> LoginInfo { get; set; }
        public DbSet<Lesson> Lesson { get; set; }
        public DbSet<MediaType> MediaType { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Flashcard> Flashcard { get; set; }
        public DbSet<Quiz> Quiz { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<Answer> Answer { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<LessonTag> LessonTag { get; set; }
        public DbSet<FlashcardLog> FlashcardLog { get; set; }
        public DbSet<Like> Like { get; set; }
    }
}
