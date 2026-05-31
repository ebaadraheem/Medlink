using MedLink.Model.Data;
using MedLink.Model.Enums;
using MedLink.Presenter.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Presenter.Presenters;

public class AdminDashboardPresenter
{
    private readonly AppDbContext _db;
    public AdminDashboardPresenter(AppDbContext db) => _db = db;

    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        // Explicitly set the Kind to UTC to satisfy PostgreSQL's strict timezone rules
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var weekStats = new List<DailyStatViewModel>();
        for (int i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var count = await _db.Appointments.CountAsync(a => a.AppointmentDate.Date == day);
            weekStats.Add(new DailyStatViewModel { Day = day.ToString("ddd"), Count = count });
        }

        var recent = await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
            .Include(a => a.Patient)
            .Include(a => a.TimeSlot)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
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
                Fee = a.Fee,
                IsPaid = a.IsPaid
            }).ToListAsync();

        return new AdminDashboardViewModel
        {
            TotalPatients = await _db.Patients.CountAsync(),
            TotalDoctors = await _db.Doctors.CountAsync(d => d.IsAvailable),
            TodayAppointments = await _db.Appointments.CountAsync(a => a.AppointmentDate.Date == today),
            PendingAppointments = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
            CompletedThisMonth = await _db.Appointments.CountAsync(a => a.AppointmentDate >= monthStart && a.Status == AppointmentStatus.Completed),
            CancelledThisMonth = await _db.Appointments.CountAsync(a => a.AppointmentDate >= monthStart && a.Status == AppointmentStatus.Cancelled),
            WeeklyStats = weekStats,
            RecentAppointments = recent
        };
    }
}