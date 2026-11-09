using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
       : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Registration> Registrations => Set<Registration>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<Models.Route> Routes => Set<Models.Route>();
        public DbSet<Weather> WeatherForecasts => Set<Weather>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(u => u.UserID);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Role>().HasKey(r => r.RoleID);

            modelBuilder.Entity<UserRole>().ToTable("UserRole");
            modelBuilder.Entity<UserRole>().HasKey(ur => ur.UserRoleID);
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserID, ur.RoleID }).IsUnique();

            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Category>().HasKey(c => c.CategoryID);

            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Event>().HasKey(e => e.EventID);
            modelBuilder.Entity<Event>()
                .Property(e => e.EntryFee).HasPrecision(10, 2);

            modelBuilder.Entity<Registration>().ToTable("Registration");
            modelBuilder.Entity<Registration>()
                .HasKey(r => r.RegistrationID);
            modelBuilder.Entity<Registration>()
                .HasIndex(r => new { r.UserID, r.EventID }).IsUnique();

            modelBuilder.Entity<Result>().ToTable("Results");
            modelBuilder.Entity<Result>().HasKey(r => r.ResultID);
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.RegistrationID).IsUnique();
            modelBuilder.Entity<Result>()
                .Property(r => r.Pace).HasPrecision(8, 2);

            modelBuilder.Entity<Models.Route>().ToTable("Route");
            modelBuilder.Entity<Models.Route>().HasKey(r => r.RouteID);
            modelBuilder.Entity<Models.Route>()
                .Property(r => r.DistanceKm).HasPrecision(7, 2);

            modelBuilder.Entity<Weather>().ToTable("Weather");
            modelBuilder.Entity<Weather>().HasKey(w => w.WeatherID);
            modelBuilder.Entity<Weather>()
                .HasIndex(w => new { w.EventID, w.ForecastDate })
                .IsUnique();

            // A user can have multiple roles.
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID);

            // Each event has an organiser and one category.
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Each registration belongs to one user and one event.
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            // A registration may have one result.
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Registration)
                .WithOne(r => r.Result)
                .HasForeignKey<Result>(r => r.RegistrationID)
                .OnDelete(DeleteBehavior.Restrict);

            // Each event has at most one route.
            modelBuilder.Entity<Models.Route>()
                .HasOne(r => r.Event)
                .WithOne(e => e.Route)
                .HasForeignKey<Models.Route>(r => r.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            // Each event can have multiple forecast dates.
            modelBuilder.Entity<Weather>()
                .HasOne(w => w.Event)
                .WithMany(e => e.WeatherForecasts)
                .HasForeignKey(w => w.EventID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
