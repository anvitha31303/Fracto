using Fracto.Models;
using Microsoft.EntityFrameworkCore;

namespace Fracto.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Specialization> Specializations { get; set; }

public DbSet<Appointment> Appointments { get; set; }
       // public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Doctor>()
       .HasOne(d => d.Specialization)
       .WithMany(s => s.Doctors)
       .HasForeignKey(d => d.SpecializationId);

            modelBuilder.Entity<Specialization>().HasData(
                new Specialization { SpecializationId = 1, SpecializationName = "Cardiology" },
                new Specialization { SpecializationId = 2, SpecializationName = "Dermatology" }
            );

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor { DoctorId = 1, Name = "Dr. Smith", SpecializationId = 1, City = "New York", Rating = 4.5 },
                new Doctor { DoctorId = 2, Name = "Dr. Johnson", SpecializationId = 2, City = "Boston", Rating = 4.2 }
            );
            // Appointment -> Doctor relation
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId);

            // Appointment -> User relation
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            // modelBuilder.Entity<DoctorAvailability>()
            //     .HasOne<Doctor>() // no navigation property, so generic type
            //     .WithMany()
            //     .HasForeignKey(d => d.DoctorId)
            //     .OnDelete(DeleteBehavior.NoAction); // prevents cascade path issues
modelBuilder.Entity<Rating>()
        .HasOne(r => r.Doctor)
        .WithMany()
        .HasForeignKey(r => r.DoctorId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Rating>()
        .HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Cascade);
            // Seed Admin
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = "$2a$11$TqN28.OFjtbJmTnDwoMbBuyxMMR5rQWxw1YxwvRLhGpx.9LSE/pC6",
                Role = "Admin"
            });
        }
    }
}
