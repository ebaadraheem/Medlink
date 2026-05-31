using MedLink.Model.Data;
using MedLink.Model.Entities;
using MedLink.Model.Enums;
using MedLink.Presenter.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Presenter.Presenters;

public class AdminAppointmentPresenter
{
    private readonly AppDbContext _db;
    public AdminAppointmentPresenter(AppDbContext db) => _db = db;

    public async Task<List<AppointmentViewModel>> GetAllAsync(string? statusFilter = null)
    {
        var query = _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
            .Include(a => a.Patient)
            .Include(a => a.TimeSlot)
            .AsQueryable();

        if (statusFilter != null && Enum.TryParse<AppointmentStatus>(statusFilter, out var s))
            query = query.Where(a => a.Status == s);

        return await query
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentViewModel
            {
                Id = a.Id,
                AppointmentNumber = a.AppointmentNumber,
                DoctorName = a.Doctor.Name,
                DoctorSpecialty = a.Doctor.Specialty.Name,
                PatientName = a.Patient.FullName,
                AppointmentDate = a.AppointmentDate,
                TimeDisplay = $"{a.TimeSlot.StartTime:hh\\:mm} - {a.TimeSlot.EndTime:hh\\:mm}",
                Status = a.Status,
                Reason = a.Reason,
                Fee = a.Fee,
                IsPaid = a.IsPaid,
                HasHealthRecord = a.HealthRecord != null
            }).ToListAsync();
    }

    public async Task UpdateStatusAsync(int id, AppointmentStatus status, string? notes, string? diagnosis, string? prescription, decimal? weight, decimal? height, string? bp, decimal? temp)
    {
        var appt = await _db.Appointments.FindAsync(id);
        if (appt == null) return;
        appt.Status = status;
        if (notes != null) appt.Notes = notes;
        if (diagnosis != null) appt.Diagnosis = diagnosis;
        if (prescription != null) appt.Prescription = prescription;
        appt.UpdatedAt = DateTime.UtcNow;

        // Auto-create or Update health record when completing
        if (status == AppointmentStatus.Completed && !string.IsNullOrEmpty(diagnosis))
        {
            var record = await _db.HealthRecords.FirstOrDefaultAsync(h => h.AppointmentId == id);
            if (record == null)
            {
                await _db.HealthRecords.AddAsync(new HealthRecord
                {
                    AppointmentId = appt.Id,
                    PatientId = appt.PatientId,
                    Diagnosis = diagnosis ?? "",
                    Prescription = prescription ?? "",
                    DoctorNotes = notes ?? "",
                    VisitDate = appt.AppointmentDate,
                    Weight = weight,
                    Height = height,
                    BloodPressure = bp,
                    Temperature = temp
                });
            }
            else
            {
                record.Diagnosis = diagnosis ?? "";
                record.Prescription = prescription ?? "";
                record.DoctorNotes = notes ?? "";
                record.Weight = weight;
                record.Height = height;
                record.BloodPressure = bp;
                record.Temperature = temp;
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task<AppointmentViewModel?> GetDetailAsync(int id)
    {
        return await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
            .Include(a => a.Patient)
            .Include(a => a.TimeSlot)
            .Include(a => a.HealthRecord)
            .Where(a => a.Id == id)
            .Select(a => new AppointmentViewModel
            {
                Id = a.Id,
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
                HealthRecordId = a.HealthRecord != null ? a.HealthRecord.Id : null,
                Weight = a.HealthRecord != null ? a.HealthRecord.Weight : null,
                Height = a.HealthRecord != null ? a.HealthRecord.Height : null,
                BloodPressure = a.HealthRecord != null ? a.HealthRecord.BloodPressure : null,
                Temperature = a.HealthRecord != null ? a.HealthRecord.Temperature : null
            }).FirstOrDefaultAsync();
    }
}