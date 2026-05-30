namespace MedLink.Model.Entities;

public class HealthRecord : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public string Diagnosis { get; set; } = string.Empty;
    public string Prescription { get; set; } = string.Empty;
    public string DoctorNotes { get; set; } = string.Empty;
    public string? FollowUpInstructions { get; set; }
    public DateTime VisitDate { get; set; }
    public decimal? Weight { get; set; }  // kg
    public decimal? Height { get; set; } // cm
    public string? BloodPressure { get; set; }
    public decimal? Temperature { get; set; } // celsius
}