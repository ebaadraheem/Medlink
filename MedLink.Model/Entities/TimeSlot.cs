namespace MedLink.Model.Entities;

public class TimeSlot : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; } = true; // weekly recurring
    public DateTime? SpecificDate { get; set; }   // for one-off slots
    public bool IsBooked { get; set; } = false;

    public Appointment? Appointment { get; set; }
}