using MedLink.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Presenter.ViewModels;

// ─── Specialty ───────────────────────────────────────────────────────────────
public class SpecialtyViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-stethoscope";
    public int DoctorCount { get; set; }
}

// ─── Doctor ───────────────────────────────────────────────────────────────────
public class DoctorCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsAvailable { get; set; }
    public bool HasPhoto { get; set; }
}

public class DoctorDetailViewModel : DoctorCardViewModel
{
    public string Bio { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<TimeSlotViewModel> AvailableSlots { get; set; } = new();
    public List<ReviewViewModel> Reviews { get; set; } = new();
}

// ─── TimeSlot ─────────────────────────────────────────────────────────────────
public class TimeSlotViewModel
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName => DayOfWeek.ToString();
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string TimeDisplay => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    public bool IsBooked { get; set; }
}

// ─── Appointment ──────────────────────────────────────────────────────────────
public class BookAppointmentViewModel
{
    [Required]
    public int DoctorId { get; set; }
    [Required]
    public int TimeSlotId { get; set; }
    [Required]
    public DateTime AppointmentDate { get; set; }
    [MaxLength(500)]
    public string? Reason { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialty { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public string SlotDisplay { get; set; } = string.Empty;
}

public class AppointmentViewModel
{
    public int Id { get; set; }
    public string AppointmentNumber { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialty { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string TimeDisplay { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public string StatusLabel => Status.ToString();
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public decimal Fee { get; set; }
    public bool IsPaid { get; set; }
    public bool HasHealthRecord { get; set; }
    public int? HealthRecordId { get; set; }
}

// ─── Health Record ────────────────────────────────────────────────────────────
public class HealthRecordViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Prescription { get; set; } = string.Empty;
    public string DoctorNotes { get; set; } = string.Empty;
    public string? FollowUpInstructions { get; set; }
    public DateTime VisitDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? BloodPressure { get; set; }
    public decimal? Temperature { get; set; }
    public string AppointmentNumber { get; set; } = string.Empty;
}

// ─── Review ───────────────────────────────────────────────────────────────────
public class ReviewViewModel
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; }
}

public class SubmitReviewViewModel
{
    [Required]
    public int DoctorId { get; set; }
    [Required, Range(1, 5)]
    public int Rating { get; set; }
    [MaxLength(1000)]
    public string? Comment { get; set; }
}

// ─── Patient Profile ──────────────────────────────────────────────────────────
public class PatientProfileViewModel
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string StudentId { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string Department { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    [Required]
    public DateTime DateOfBirth { get; set; }
    [MaxLength(5)]
    public string BloodGroup { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? EmergencyContact { get; set; }
    [MaxLength(2000)]
    public string? MedicalHistory { get; set; }
}

// ─── Admin ViewModels ─────────────────────────────────────────────────────────
public class AdminDashboardViewModel
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TodayAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedThisMonth { get; set; }
    public int CancelledThisMonth { get; set; }
    public List<DailyStatViewModel> WeeklyStats { get; set; } = new();
    public List<AppointmentViewModel> RecentAppointments { get; set; } = new();
}

public class DailyStatViewModel
{
    public string Day { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AdminDoctorViewModel
{
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Bio { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Qualifications { get; set; } = string.Empty;
    [Required, Range(0, 60)]
    public int ExperienceYears { get; set; }
    [Required, Range(0, 100000)]
    public decimal ConsultationFee { get; set; }
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    [Required]
    public int SpecialtyId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<SpecialtyViewModel> Specialties { get; set; } = new();
}