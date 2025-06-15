using Microsoft.EntityFrameworkCore;
using MovieHall.Models;

namespace MovieHall.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Genre> Genre { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<WatchedWith> WatchedWith { get; set; }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<MovieWatchedWith> MovieWatchedWiths { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieGenre>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });

            modelBuilder.Entity<MovieWatchedWith>()
                .HasKey(mw => new { mw.MovieId, mw.WatchedWithId });

            modelBuilder.Entity<MovieGenre>()
                .HasOne(mg => mg.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(mg => mg.MovieId);

            modelBuilder.Entity<MovieGenre>()
                .HasOne(mg => mg.Genre)
                .WithMany(g => g.MovieGenres)
                .HasForeignKey(mg => mg.GenreId);

            modelBuilder.Entity<MovieWatchedWith>()
                .HasOne(mw => mw.Movie)
                .WithMany(m => m.MovieWatchedWiths)
                .HasForeignKey(mw => mw.MovieId);

            modelBuilder.Entity<MovieWatchedWith>()
                .HasOne(mw => mw.WatchedWith)
                .WithMany(w => w.MovieWatchedWiths)
                .HasForeignKey(mw => mw.WatchedWithId);
        }

    }
}
