using MedLink.Model.Data;
using MedLink.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Model.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByUserIdAsync(string userId);
    Task<Patient?> GetWithAppointmentsAsync(int patientId);
    Task<Patient?> GetWithHealthRecordsAsync(int patientId);
}

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext db) : base(db) { }

    public async Task<Patient?> GetByUserIdAsync(string userId)
        => await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<Patient?> GetWithAppointmentsAsync(int patientId)
        => await _db.Patients
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.Specialty)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.TimeSlot)
            .FirstOrDefaultAsync(p => p.Id == patientId);

    public async Task<Patient?> GetWithHealthRecordsAsync(int patientId)
        => await _db.Patients
            .Include(p => p.HealthRecords)
                .ThenInclude(h => h.Appointment)
                    .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(p => p.Id == patientId);
}