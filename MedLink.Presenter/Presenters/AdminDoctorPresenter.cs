using MedLink.Model.Data;
using MedLink.Model.Entities;
using MedLink.Presenter.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Presenter.Presenters;

public class AdminDoctorPresenter
{
    private readonly AppDbContext _db;
    public AdminDoctorPresenter(AppDbContext db) => _db = db;

    public async Task<List<DoctorCardViewModel>> GetAllAsync()
        => await _db.Doctors
            .Include(d => d.Specialty)
            .Select(d => new DoctorCardViewModel
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

    public async Task<AdminDoctorViewModel> GetForEditAsync(int id)
    {
        var d = await _db.Doctors.FindAsync(id) ?? throw new KeyNotFoundException();
        return new AdminDoctorViewModel
        {
            Id = d.Id, Name = d.Name, Bio = d.Bio, Qualifications = d.Qualifications,
            ExperienceYears = d.ExperienceYears, ConsultationFee = d.ConsultationFee,
            Phone = d.Phone, Email = d.Email, SpecialtyId = d.SpecialtyId, IsAvailable = d.IsAvailable,
            Specialties = await GetSpecialtyListAsync()
        };
    }

    public async Task<List<SpecialtyViewModel>> GetSpecialtyListAsync()
        => await _db.Specialties.Select(s => new SpecialtyViewModel { Id = s.Id, Name = s.Name, Icon = s.Icon }).ToListAsync();

    public async Task CreateAsync(AdminDoctorViewModel vm, byte[]? photo, string? contentType)
    {
        var doctor = new Doctor
        {
            Name = vm.Name, Bio = vm.Bio, Qualifications = vm.Qualifications,
            ExperienceYears = vm.ExperienceYears, ConsultationFee = vm.ConsultationFee,
            Phone = vm.Phone, Email = vm.Email, SpecialtyId = vm.SpecialtyId, IsAvailable = vm.IsAvailable,
            PhotoData = photo, PhotoContentType = contentType
        };
        await _db.Doctors.AddAsync(doctor);

        // Seed default time slots Mon-Fri
        await _db.SaveChangesAsync();
        var slots = new List<TimeSlot>();
        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
        {
            foreach (var hour in new[] { 9, 10, 11, 14, 15, 16 })
                slots.Add(new TimeSlot { DoctorId = doctor.Id, DayOfWeek = day, StartTime = new TimeSpan(hour, 0, 0), EndTime = new TimeSpan(hour, 30, 0) });
        }
        await _db.TimeSlots.AddRangeAsync(slots);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AdminDoctorViewModel vm, byte[]? photo, string? contentType)
    {
        var doctor = await _db.Doctors.FindAsync(vm.Id) ?? throw new KeyNotFoundException();
        doctor.Name = vm.Name; doctor.Bio = vm.Bio; doctor.Qualifications = vm.Qualifications;
        doctor.ExperienceYears = vm.ExperienceYears; doctor.ConsultationFee = vm.ConsultationFee;
        doctor.Phone = vm.Phone; doctor.Email = vm.Email; doctor.SpecialtyId = vm.SpecialtyId;
        doctor.IsAvailable = vm.IsAvailable; doctor.UpdatedAt = DateTime.UtcNow;
        if (photo != null) { doctor.PhotoData = photo; doctor.PhotoContentType = contentType; }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor != null) { _db.Doctors.Remove(doctor); await _db.SaveChangesAsync(); }
    }
}