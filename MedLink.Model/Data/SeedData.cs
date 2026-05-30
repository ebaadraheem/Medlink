using MedLink.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Model.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        if (await db.Specialties.AnyAsync()) return;

        var specialties = new List<Specialty>
        {
            new() { Name = "General Medicine", Description = "Primary healthcare and general consultations", Icon = "fa-stethoscope" },
            new() { Name = "Cardiology", Description = "Heart and cardiovascular system", Icon = "fa-heart-pulse" },
            new() { Name = "Orthopedics", Description = "Bones, joints and musculoskeletal system", Icon = "fa-bone" },
            new() { Name = "Dermatology", Description = "Skin, hair and nail conditions", Icon = "fa-microscope" },
            new() { Name = "Psychiatry", Description = "Mental health and behavioral disorders", Icon = "fa-brain" },
            new() { Name = "Ophthalmology", Description = "Eye care and vision health", Icon = "fa-eye" },
            new() { Name = "Dental", Description = "Oral health and dental care", Icon = "fa-tooth" },
            new() { Name = "Gynecology", Description = "Women's reproductive health", Icon = "fa-venus" },
        };

        await db.Specialties.AddRangeAsync(specialties);
        await db.SaveChangesAsync();

        var doctors = new List<Doctor>
        {
            new()
            {
                Name = "Dr. Sarah Ahmed",
                Bio = "Experienced general physician with 12 years of practice. Specializes in preventive care and chronic disease management.",
                Qualifications = "MBBS, FCPS (Medicine)",
                ExperienceYears = 12,
                ConsultationFee = 500,
                Phone = "0300-1234567",
                Email = "sarah.ahmed@medlink.edu",
                SpecialtyId = specialties[0].Id,
                AverageRating = 4.8,
                ReviewCount = 124,
                IsAvailable = true
            },
            new()
            {
                Name = "Dr. Hassan Mirza",
                Bio = "Board-certified cardiologist specializing in interventional cardiology and heart failure management.",
                Qualifications = "MBBS, MD (Cardiology), FACC",
                ExperienceYears = 18,
                ConsultationFee = 1500,
                Phone = "0300-2345678",
                Email = "hassan.mirza@medlink.edu",
                SpecialtyId = specialties[1].Id,
                AverageRating = 4.9,
                ReviewCount = 87,
                IsAvailable = true
            },
            new()
            {
                Name = "Dr. Fatima Khan",
                Bio = "Orthopedic surgeon with expertise in sports injuries and joint replacement surgery.",
                Qualifications = "MBBS, MS (Orthopedics)",
                ExperienceYears = 10,
                ConsultationFee = 1200,
                Phone = "0300-3456789",
                Email = "fatima.khan@medlink.edu",
                SpecialtyId = specialties[2].Id,
                AverageRating = 4.7,
                ReviewCount = 65,
                IsAvailable = true
            },
            new()
            {
                Name = "Dr. Ali Raza",
                Bio = "Dermatologist specializing in acne, eczema, and cosmetic dermatology.",
                Qualifications = "MBBS, DDVL",
                ExperienceYears = 8,
                ConsultationFee = 800,
                Phone = "0300-4567890",
                Email = "ali.raza@medlink.edu",
                SpecialtyId = specialties[3].Id,
                AverageRating = 4.6,
                ReviewCount = 93,
                IsAvailable = true
            },
            new()
            {
                Name = "Dr. Nadia Hussain",
                Bio = "Psychiatrist focused on student mental health, anxiety, depression and stress management.",
                Qualifications = "MBBS, MRCPsych",
                ExperienceYears = 9,
                ConsultationFee = 1000,
                Phone = "0300-5678901",
                Email = "nadia.hussain@medlink.edu",
                SpecialtyId = specialties[4].Id,
                AverageRating = 4.9,
                ReviewCount = 141,
                IsAvailable = true
            },
            new()
            {
                Name = "Dr. Usman Sheikh",
                Bio = "Ophthalmologist with expertise in refractive surgery, glaucoma and retinal diseases.",
                Qualifications = "MBBS, FCPS (Ophthalmology)",
                ExperienceYears = 14,
                ConsultationFee = 1100,
                Phone = "0300-6789012",
                Email = "usman.sheikh@medlink.edu",
                SpecialtyId = specialties[5].Id,
                AverageRating = 4.7,
                ReviewCount = 58,
                IsAvailable = true
            },
        };

        await db.Doctors.AddRangeAsync(doctors);
        await db.SaveChangesAsync();

        // Seed time slots for each doctor (Mon-Fri, morning and afternoon)
        var slots = new List<TimeSlot>();
        foreach (var doctor in doctors)
        {
            foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
            {
                // Morning slots: 9AM, 10AM, 11AM
                slots.Add(new TimeSlot { DoctorId = doctor.Id, DayOfWeek = day, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(9, 30, 0) });
                slots.Add(new TimeSlot { DoctorId = doctor.Id, DayOfWeek = day, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 30, 0) });
                slots.Add(new TimeSlot { DoctorId = doctor.Id, DayOfWeek = day, StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(11, 30, 0) });
                // Afternoon slots: 2PM, 3PM, 4PM
                slots.Add(new TimeSlot { DoctorId = doctor.Id, DayOfWeek = day, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(14, 30, 0) });
                slots.Add(new TimeSlot { DoctorId = doctor.Id, DayOfWeek = day, StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(15, 30, 0) });
                slots.Add(new TimeSlot { DoctorId = doctor.Id, DayOfWeek = day, StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(16, 30, 0) });
            }
        }

        await db.TimeSlots.AddRangeAsync(slots);
        await db.SaveChangesAsync();
    }
}