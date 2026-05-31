using MedLink.Model.Data;
using MedLink.Presenter.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Presenter.Presenters;

public class DoctorListPresenter
{
    private readonly AppDbContext _db;

    public DoctorListPresenter(AppDbContext db) => _db = db;

    public async Task<List<DoctorCardViewModel>> GetDoctorsAsync(string? search, int? specialtyId, string? sort)
    {
        var query = _db.Doctors
            .Include(d => d.Specialty)
            .Where(d => d.IsAvailable)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();
            query = query.Where(d => 
                (d.Name != null && d.Name.ToLower().Contains(searchTerm)) || 
                (d.Qualifications != null && d.Qualifications.ToLower().Contains(searchTerm)) || 
                (d.Bio != null && d.Bio.ToLower().Contains(searchTerm)));
        }

        if (specialtyId.HasValue)
            query = query.Where(d => d.SpecialtyId == specialtyId.Value);

        query = sort switch
        {
            "rating" => query.OrderByDescending(d => d.AverageRating),
            "fee_asc" => query.OrderBy(d => d.ConsultationFee),
            "fee_desc" => query.OrderByDescending(d => d.ConsultationFee),
            "experience" => query.OrderByDescending(d => d.ExperienceYears),
            _ => query.OrderBy(d => d.Name)
        };

        return await query.Select(d => new DoctorCardViewModel
        {
            Id = d.Id,
            Name = d.Name,
            Qualifications = d.Qualifications,
            SpecialtyName = d.Specialty.Name,
            ExperienceYears = d.ExperienceYears,
            ConsultationFee = d.ConsultationFee,
            AverageRating = d.AverageRating,
            ReviewCount = d.ReviewCount,
            IsAvailable = d.IsAvailable,
            HasPhoto = d.PhotoData != null
        }).ToListAsync();
    }

    public async Task<List<SpecialtyViewModel>> GetSpecialtiesAsync()
    {
        return await _db.Specialties
            .Select(s => new SpecialtyViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Icon = s.Icon,
                DoctorCount = s.Doctors.Count(d => d.IsAvailable)
            }).ToListAsync();
    }

    public async Task<DoctorDetailViewModel?> GetDoctorDetailAsync(int doctorId)
    {
        var doctor = await _db.Doctors
            .Include(d => d.Specialty)
            .Include(d => d.TimeSlots)
            .Include(d => d.Reviews)
                .ThenInclude(r => r.Patient)
            .AsSplitQuery() // <--- ADD THIS EXACT LINE HERE!
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        if (doctor == null) return null;

        return new DoctorDetailViewModel
        {
            Id = doctor.Id,
            Name = doctor.Name,
            Bio = doctor.Bio,
            Qualifications = doctor.Qualifications,
            SpecialtyName = doctor.Specialty.Name,
            ExperienceYears = doctor.ExperienceYears,
            ConsultationFee = doctor.ConsultationFee,
            AverageRating = doctor.AverageRating,
            ReviewCount = doctor.ReviewCount,
            IsAvailable = doctor.IsAvailable,
            Phone = doctor.Phone,
            Email = doctor.Email,
            HasPhoto = doctor.PhotoData != null,
            AvailableSlots = doctor.TimeSlots
                .Where(t => !t.IsBooked)
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .Select(t => new TimeSlotViewModel
                {
                    Id = t.Id,
                    DayOfWeek = t.DayOfWeek,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    IsBooked = t.IsBooked
                }).ToList(),
            Reviews = doctor.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    DoctorId = r.DoctorId,
                    PatientName = r.Patient.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsVerified = r.IsVerified
                }).ToList()
        };
    }
}