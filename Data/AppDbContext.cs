using Microsoft.EntityFrameworkCore;
using HospitalApi.Models;

namespace HospitalApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Bill> Bills { get; set; } = null!;
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User → Role (Many-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient → CreatedByUser (Many-to-One)
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.CreatedByUser)
                .WithMany(u => u.CreatedPatients)
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Doctor → CreatedByUser (Many-to-One)
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.CreatedByUser)
                .WithMany(u => u.CreatedDoctors)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Appointment → Patient
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment → Doctor
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment → CreatedByUser
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.CreatedByUser)
                .WithMany(u => u.CreatedAppointments)
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bill → Appointment
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Appointment)
                .WithMany()
                .HasForeignKey(b => b.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bill → Patient
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Patient)
                .WithMany()
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bill → Doctor
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Doctor)
                .WithMany()
                .HasForeignKey(b => b.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bill → CreatedByUser
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.CreatedByUser)
                .WithMany(u => u.CreatedBills)
                .HasForeignKey(b => b.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Bill → UpdatedByUser
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.UpdatedByUser)
                .WithMany(u => u.UpdatedBills)
                .HasForeignKey(b => b.UpdatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
