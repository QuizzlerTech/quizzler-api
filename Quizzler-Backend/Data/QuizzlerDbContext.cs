using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Models;
using System.Reflection.Emit;

public class QuizzlerDbContext : DbContext
{
    public QuizzlerDbContext(DbContextOptions<QuizzlerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().OwnsOne(u => u.LoginInfo);
        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.User)
            .WithMany(u => u.Lesson)
            .HasForeignKey(l => l.LessonOwner);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(Environment.GetEnvironmentVariable("DbConnection"), new MySqlServerVersion(new Version(8, 0, 21)));
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
}
