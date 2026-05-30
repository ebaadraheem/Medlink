using MedLink.Model.Enums;

namespace MedLink.Model.Entities;

public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public int TimeSlotId { get; set; }
    public TimeSlot TimeSlot { get; set; } = null!;

    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Reason { get; set; }        // patient-entered reason
    public string? Notes { get; set; }         // doctor notes after visit
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public decimal Fee { get; set; }
    public bool IsPaid { get; set; } = false;
    public string AppointmentNumber { get; set; } = string.Empty; // e.g. APT-2024-00123

    public HealthRecord? HealthRecord { get; set; }
}