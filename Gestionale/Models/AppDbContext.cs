using ClickClok.Models;
using Microsoft.EntityFrameworkCore;

namespace ClickClok.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura la relazione tra Appointment e User
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User) // La proprietà di navigazione
                .WithMany(u => u.Appointments) // La collezione nel lato User
                .HasForeignKey(a => a.UserId) // La chiave esterna
                .OnDelete(DeleteBehavior.Cascade); // Comportamento di eliminazione

            // Configura Username come case-insensitive per SQLite
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasColumnType("TEXT COLLATE NOCASE"); // Usa NOCASE per case-insensitivity
        }
    }
}
