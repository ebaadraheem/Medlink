namespace MedLink.Model.Entities;

public class Doctor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public byte[]? PhotoData { get; set; }
    public string? PhotoContentType { get; set; }
    public bool IsAvailable { get; set; } = true;
    public double AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;

    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;

    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();
}