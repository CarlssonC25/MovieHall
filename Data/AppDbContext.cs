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

        public DbSet<Anime> Animes { get; set; }
        public DbSet<AnimeGenre> AnimeGenres { get; set; }
        public DbSet<AnimeWatchedWith> AnimeWatchedWiths { get; set; }

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



            modelBuilder.Entity<AnimeGenre>()
                .HasKey(ag => new { ag.AnimeId, ag.GenreId });

            modelBuilder.Entity<AnimeWatchedWith>()
                .HasKey(aw => new { aw.AnimeId, aw.WatchedWithId });

            modelBuilder.Entity<AnimeGenre>()
                .HasOne(ag => ag.Anime)
                .WithMany(m => m.AnimeGenres)
                .HasForeignKey(ag => ag.AnimeId);

            modelBuilder.Entity<AnimeGenre>()
                .HasOne(ag => ag.Genre)
                .WithMany(g => g.AnimeGenres)
                .HasForeignKey(ag => ag.GenreId);

            modelBuilder.Entity<AnimeWatchedWith>()
                .HasOne(aw => aw.Anime)
                .WithMany(m => m.AnimeWatchedWiths)
                .HasForeignKey(aw => aw.AnimeId);

            modelBuilder.Entity<AnimeWatchedWith>()
                .HasOne(aw => aw.WatchedWith)
                .WithMany(w => w.AnimeWatchedWiths)
                .HasForeignKey(aw => aw.WatchedWithId);
        }

    }
}
