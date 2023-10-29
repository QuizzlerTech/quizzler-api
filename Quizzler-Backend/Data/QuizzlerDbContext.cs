using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Models;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore.Proxies;
using MySql.EntityFrameworkCore;
using MySql.Data.MySqlClient;

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
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(quiz => quiz.Questions)
                .WithOne(q => q.Quiz)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // Question Configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId);
            entity.Property(e => e.QuizId).IsRequired();
            entity.Property(e => e.QuestionText).HasMaxLength(255);
            entity.HasOne(e => e.Media)
                  .WithMany()
                  .HasForeignKey(e => e.QuestionMediaId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(question => question.Answers)
                .WithOne(q => q.Question)
                .HasForeignKey(q => q.QuestionId)
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
            entity.Property(e => e.Path).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileSize).IsRequired();
            entity.HasOne(e => e.MediaType)
                  .WithMany()
                  .HasForeignKey(e => e.MediaTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Uploader)
                  .WithMany()
                  .HasForeignKey(e => e.UploaderId)
                  .OnDelete(DeleteBehavior.Restrict);

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

            entity.Property(e => e.IsPublic)
                .IsRequired();

            entity.HasMany(l => l.Flashcards)
                .WithOne(f => f.Lesson)
                .HasForeignKey(f => f.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(l => l.LessonTags)
                  .WithOne(lt => lt.Lesson)
                  .HasForeignKey(lt => lt.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.Lesson)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Media)
                .WithMany()
                .HasForeignKey(l => l.LessonMediaId)
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
            entity.HasOne(l => l.AnswerMedia)
                .WithMany()
                .HasForeignKey(l => l.AnswerMediaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.QuestionMedia)
                .WithMany()
                .HasForeignKey(l => l.QuestionMediaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Answer Configuration
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId);
            entity.Property(e => e.QuestionId).IsRequired();
            entity.Property(e => e.AnswerText).HasMaxLength(255);
            entity.Property(e => e.IsCorrect).IsRequired();
            entity.HasOne(l => l.Media)
                .WithMany()
                .HasForeignKey(l => l.AnswerMediaId)
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
            entity.HasOne(f => f.Flashcard)
                .WithMany(f => f.FlashcardLog)
                .HasForeignKey(f => f.FlashcardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        optionsBuilder.UseMySQL(Environment.GetEnvironmentVariable("DbConnection"));
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
}
