using MedLink.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Model.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();
    public DbSet<DoctorReview> DoctorReviews => Set<DoctorReview>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Doctor>()
            .HasMany(d => d.TimeSlots)
            .WithOne(t => t.Doctor)
            .HasForeignKey(t => t.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Doctor>()
            .HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Patient>()
            .HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Appointment>()
            .HasOne(a => a.TimeSlot)
            .WithOne(t => t.Appointment)
            .HasForeignKey<Appointment>(a => a.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.HealthRecord)
            .WithOne(h => h.Appointment)
            .HasForeignKey<HealthRecord>(h => h.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Doctor>()
            .Property(d => d.ConsultationFee)
            .HasColumnType("numeric(10,2)");

        builder.Entity<Appointment>()
            .Property(a => a.Fee)
            .HasColumnType("numeric(10,2)");

        builder.Entity<HealthRecord>()
            .Property(h => h.Weight)
            .HasColumnType("numeric(5,2)");

        builder.Entity<HealthRecord>()
            .Property(h => h.Height)
            .HasColumnType("numeric(5,2)");

        builder.Entity<HealthRecord>()
            .Property(h => h.Temperature)
            .HasColumnType("numeric(4,1)");
    }
}