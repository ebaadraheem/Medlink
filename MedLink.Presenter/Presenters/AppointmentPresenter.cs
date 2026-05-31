using MedLink.Model.Data;
using MedLink.Model.Entities;
using MedLink.Model.Enums;
using MedLink.Model.Repositories;
using MedLink.Presenter.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Presenter.Presenters;

public class AppointmentPresenter
{
    private readonly AppDbContext _db;
    private readonly IPatientRepository _patients;
    private readonly IEmailSender _email;

    public AppointmentPresenter(AppDbContext db, IPatientRepository patients, IEmailSender email)
    {
        _db = db;
        _patients = patients;
        _email = email;
    }

    public async Task<(bool success, string message, int appointmentId)> BookAsync(BookAppointmentViewModel model, string userId)
    {
        var patient = await _patients.GetByUserIdAsync(userId);
        if (patient == null) return (false, "Please complete your profile before booking.", 0);

        var slot = await _db.TimeSlots.FindAsync(model.TimeSlotId);
        if (slot == null || slot.IsBooked) return (false, "This time slot is no longer available.", 0);

        var doctor = await _db.Doctors.Include(d => d.Specialty).FirstOrDefaultAsync(d => d.Id == model.DoctorId);
        if (doctor == null) return (false, "Doctor not found.", 0);

        model.AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate, DateTimeKind.Utc);
        // Prevent double-booking same patient/doctor/date
        var alreadyBooked = await _db.Appointments.AnyAsync(a =>
            a.PatientId == patient.Id &&
            a.DoctorId == model.DoctorId &&
            a.AppointmentDate.Date == model.AppointmentDate.Date &&
            a.Status != AppointmentStatus.Cancelled);
        if (alreadyBooked) return (false, "You already have an appointment with this doctor on this date.", 0);

        var count = await _db.Appointments.CountAsync() + 1;
        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = model.DoctorId,
            TimeSlotId = model.TimeSlotId,
            AppointmentDate = model.AppointmentDate,
            Reason = model.Reason,
            Fee = doctor.ConsultationFee,
            Status = AppointmentStatus.Pending,
            AppointmentNumber = $"APT-{DateTime.UtcNow.Year}-{count:D5}"
        };

        slot.IsBooked = true;
        _db.TimeSlots.Update(slot);
        await _db.Appointments.AddAsync(appointment);
        await _db.SaveChangesAsync();

        // Send confirmation email
        try
        {
            var user = await _db.Users.FindAsync(userId);
            if (user?.Email != null)
                await _email.SendAppointmentConfirmationAsync(
                    user.Email, patient.FullName, doctor.Name,
                    appointment.AppointmentDate,
                    $"{slot.StartTime:hh\\:mm} - {slot.EndTime:hh\\:mm}",
                    appointment.AppointmentNumber);
        }
        catch { /* email failure should not break booking */ }

        return (true, "Appointment booked successfully!", appointment.Id);
    }

    public async Task<List<AppointmentViewModel>> GetPatientAppointmentsAsync(string userId)
    {
        var patient = await _patients.GetByUserIdAsync(userId);
        if (patient == null) return new();

        // Fetch all reviews left by this patient
        var patientReviews = await _db.DoctorReviews.Where(r => r.PatientId == patient.Id).ToListAsync();

        var appointments = await _db.Appointments
            .Where(a => a.PatientId == patient.Id)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
            .Include(a => a.TimeSlot)
            .Include(a => a.HealthRecord)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentViewModel
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                AppointmentNumber = a.AppointmentNumber,
                DoctorName = a.Doctor.Name,
                DoctorSpecialty = a.Doctor.Specialty.Name,
                PatientName = a.Patient.FullName,
                AppointmentDate = a.AppointmentDate,
                TimeDisplay = $"{a.TimeSlot.StartTime:hh\\:mm} - {a.TimeSlot.EndTime:hh\\:mm}",
                Status = a.Status,
                Reason = a.Reason,
                Notes = a.Notes,
                Diagnosis = a.Diagnosis,
                Prescription = a.Prescription,
                Fee = a.Fee,
                IsPaid = a.IsPaid,
                HasHealthRecord = a.HealthRecord != null,
                HealthRecordId = a.HealthRecord != null ? a.HealthRecord.Id : null
            }).ToListAsync();

        // Attach the review data to the respective appointments
        foreach (var appt in appointments)
        {
            var review = patientReviews.FirstOrDefault(r => r.DoctorId == appt.DoctorId);
            if (review != null)
            {
                appt.ExistingRating = review.Rating;
                appt.ExistingReviewComment = review.Comment;
            }
        }

        return appointments;
    }

    public async Task<bool> CancelAsync(int appointmentId, string userId)
    {
        var patient = await _patients.GetByUserIdAsync(userId);
        if (patient == null) return false;

        var appt = await _db.Appointments
            .Include(a => a.TimeSlot)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id);

        if (appt == null || appt.Status == AppointmentStatus.Completed) return false;

        appt.Status = AppointmentStatus.Cancelled;
        appt.TimeSlot.IsBooked = false; // free up the slot
        await _db.SaveChangesAsync();

        try
        {
            var user = await _db.Users.FindAsync(userId);
            if (user?.Email != null)
                await _email.SendCancellationEmailAsync(user.Email, patient.FullName, appt.Doctor.Name, appt.AppointmentDate);
        }
        catch { }

        return true;
    }

    public async Task<HealthRecordViewModel?> GetHealthRecordAsync(int recordId, string userId)
    {
        var patient = await _patients.GetByUserIdAsync(userId);
        if (patient == null) return null;

        return await _db.HealthRecords
            .Where(h => h.Id == recordId && h.PatientId == patient.Id)
            .Include(h => h.Appointment).ThenInclude(a => a.Doctor)
            .Select(h => new HealthRecordViewModel
            {
                Id = h.Id,
                PatientName = h.Patient.FullName,
                DoctorName = h.Appointment.Doctor.Name,
                Diagnosis = h.Diagnosis,
                Prescription = h.Prescription,
                DoctorNotes = h.DoctorNotes,
                FollowUpInstructions = h.FollowUpInstructions,
                VisitDate = h.VisitDate,
                Weight = h.Weight,
                Height = h.Height,
                BloodPressure = h.BloodPressure,
                Temperature = h.Temperature,
                AppointmentNumber = h.Appointment.AppointmentNumber
            }).FirstOrDefaultAsync();
    }
}