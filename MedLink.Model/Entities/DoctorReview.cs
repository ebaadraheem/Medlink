namespace MedLink.Model.Entities;

public class DoctorReview : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public bool IsVerified { get; set; } = false; // only patients who had appointment
}