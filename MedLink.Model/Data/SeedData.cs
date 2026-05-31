using MedLink.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace MedLink.Model.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        if (await db.Specialties.AnyAsync()) return;

        // 1. Seed Specialties
        var specialties = new List<Specialty>
        {
            new() { Name = "General Medicine", Description = "Primary healthcare", Icon = "fa-stethoscope" },
            new() { Name = "Cardiology", Description = "Heart and cardiovascular", Icon = "fa-heart-pulse" },
            new() { Name = "Orthopedics", Description = "Bones and joints", Icon = "fa-bone" },
            new() { Name = "Dermatology", Description = "Skin, hair and nails", Icon = "fa-microscope" },
            new() { Name = "Psychiatry", Description = "Mental health", Icon = "fa-brain" },
            new() { Name = "Ophthalmology", Description = "Eye care", Icon = "fa-eye" }
        };
        await db.Specialties.AddRangeAsync(specialties);
        await db.SaveChangesAsync();

        // 2. Procedurally Generate 50 Users & Patients
        var random = new Random();
        var dob = DateTime.SpecifyKind(new DateTime(1998, 1, 1), DateTimeKind.Utc);
        string[] firstNames = { "Ali", "Ayesha", "Bilal", "Fatima", "Hassan", "Zainab", "Omar", "Sara", "Usman", "Mariam" };
        string[] lastNames = { "Khan", "Ahmed", "Tariq", "Mirza", "Sheikh", "Raza", "Hussain", "Malik", "Shah", "Iqbal" };
        
        var users = new List<IdentityUser>();
        var patients = new List<Patient>();
        
        for (int i = 1; i <= 50; i++)
        {
            var userId = $"user-{i}";
            
            // Create the Base Identity User first
            users.Add(new IdentityUser
            {
                Id = userId,
                UserName = $"student{i}@medlink.edu",
                NormalizedUserName = $"STUDENT{i}@MEDLINK.EDU",
                Email = $"student{i}@medlink.edu",
                NormalizedEmail = $"STUDENT{i}@MEDLINK.EDU",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            });

            // Create the linked Patient Profile
            patients.Add(new Patient
            {
                UserId = userId,
                FullName = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}",
                StudentId = $"STU-{1000 + i}",
                Department = "General Science",
                Phone = $"0300{random.Next(1000000, 9999999)}",
                DateOfBirth = dob.AddDays(random.Next(1, 3000))
            });
        }
        
        // Save Users first to satisfy the Foreign Key constraint
        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();
        
        // Then save the Patients
        await db.Patients.AddRangeAsync(patients);
        await db.SaveChangesAsync();

        // 3. Generate 18 Doctors (3 per Specialty)
        var doctors = new List<Doctor>();
        string[] docFirstNames = { "Kamran", "Sadia", "Faisal", "Nida", "Tariq", "Hira" };
        
        foreach (var spec in specialties)
        {
            for (int i = 0; i < 3; i++)
            {
                doctors.Add(new Doctor
                {
                    Name = $"Dr. {docFirstNames[random.Next(docFirstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}",
                    Bio = $"Highly experienced specialist in {spec.Name}.",
                    Qualifications = "MBBS, FCPS",
                    ExperienceYears = random.Next(5, 25),
                    ConsultationFee = random.Next(5, 20) * 100, // Rs. 500 to 2000
                    SpecialtyId = spec.Id,
                    IsAvailable = true
                });
            }
        }
        await db.Doctors.AddRangeAsync(doctors);
        await db.SaveChangesAsync();

        // 4. Generate Hundreds of Reviews & Sync Math
        var reviews = new List<DoctorReview>();
        string[] comments = { 
            "Incredible doctor, highly recommended!", "Very professional.", "Took the time to listen to me.", 
            "Wait was long but the doctor was great.", "Solved my issue immediately.", "Best doctor on campus!" 
        };

        foreach (var doc in doctors)
        {
            int numOfReviews = random.Next(15, 40); // 15 to 40 reviews per doctor
            int totalStars = 0;

            for (int i = 0; i < numOfReviews; i++)
            {
                int rating = random.Next(3, 6); // 3 to 5 stars
                totalStars += rating;

                reviews.Add(new DoctorReview
                {
                    DoctorId = doc.Id,
                    PatientId = patients[random.Next(patients.Count)].Id,
                    Rating = rating,
                    Comment = comments[random.Next(comments.Length)],
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 100))
                });
            }
            doc.ReviewCount = numOfReviews;
            doc.AverageRating = Math.Round((double)totalStars / numOfReviews, 1);
        }

        db.Doctors.UpdateRange(doctors);
        await db.DoctorReviews.AddRangeAsync(reviews);
        
        // 5. Generate TimeSlots
        var slots = new List<TimeSlot>();
        foreach (var doc in doctors)
        {
            foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
            {
                slots.Add(new TimeSlot { DoctorId = doc.Id, DayOfWeek = day, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(9, 30, 0) });
                slots.Add(new TimeSlot { DoctorId = doc.Id, DayOfWeek = day, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 30, 0) });
            }
        }
        await db.TimeSlots.AddRangeAsync(slots);
        await db.SaveChangesAsync();
    }
}