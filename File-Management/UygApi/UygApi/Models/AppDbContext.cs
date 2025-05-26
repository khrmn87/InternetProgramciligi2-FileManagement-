using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace UygApi.Models
{
    public class AppDbContext:IdentityDbContext<AppUser, AppRole, string>
    {
        public DbSet<FileModal> Files { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Starred> Starreds { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<FileShareWith> FileShares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FileShare ilişkilendirmelerini doğru yapma
            modelBuilder.Entity<FileShareWith>()
                .HasKey(fs => fs.Id);

            modelBuilder.Entity<FileShareWith>()
                .HasOne(fs => fs.File)
                .WithMany()
                .HasForeignKey(fs => fs.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FileShareWith>()
                .HasOne(fs => fs.User)
                .WithMany()
                .HasForeignKey(fs => fs.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

