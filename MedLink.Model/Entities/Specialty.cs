namespace MedLink.Model.Entities;

public class Specialty : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-stethoscope"; // Font Awesome icon class
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}