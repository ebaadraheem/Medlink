using MedLink.Model.Data;
using MedLink.Model.Entities;
using MedLink.Model.Enums;
using MedLink.Model.Repositories;
using MedLink.Presenter.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Presenter.Presenters;

public class ReviewPresenter
{
    private readonly AppDbContext _db;
    private readonly IPatientRepository _patients;

    public ReviewPresenter(AppDbContext db, IPatientRepository patients)
    {
        _db = db;
        _patients = patients;
    }

    public async Task<(bool success, string message)> SubmitAsync(SubmitReviewViewModel vm, string userId)
    {
        var patient = await _patients.GetByUserIdAsync(userId);
        if (patient == null) return (false, "Patient profile not found.");

        // Only allow reviews from patients who had a completed appointment
        var hadAppointment = await _db.Appointments.AnyAsync(a =>
            a.PatientId == patient.Id &&
            a.DoctorId == vm.DoctorId &&
            a.Status == AppointmentStatus.Completed);

        var existing = await _db.DoctorReviews.FirstOrDefaultAsync(r => r.PatientId == patient.Id && r.DoctorId == vm.DoctorId);
        if (existing != null)
        {
            existing.Rating = vm.Rating;
            existing.Comment = vm.Comment;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            await _db.DoctorReviews.AddAsync(new DoctorReview
            {
                DoctorId = vm.DoctorId,
                PatientId = patient.Id,
                Rating = vm.Rating,
                Comment = vm.Comment,
                IsVerified = hadAppointment
            });
        }

        await _db.SaveChangesAsync();

        // Recalculate doctor's average rating
        var reviews = await _db.DoctorReviews.Where(r => r.DoctorId == vm.DoctorId).ToListAsync();
        var doctor = await _db.Doctors.FindAsync(vm.DoctorId);
        if (doctor != null)
        {
            doctor.AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;
            doctor.ReviewCount = reviews.Count;
            await _db.SaveChangesAsync();
        }

        return (true, "Review submitted successfully.");
    }
}