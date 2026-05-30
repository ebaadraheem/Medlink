using Microsoft.AspNetCore.Identity;

namespace MedLink.Model.Entities;

public class Patient : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty; // University student ID
    public string Department { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string BloodGroup { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }
    public string? MedicalHistory { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();
    public ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
}