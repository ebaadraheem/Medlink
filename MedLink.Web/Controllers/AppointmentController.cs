using MedLink.Model.Data;
using MedLink.Model.Repositories;
using MedLink.Presenter.Presenters;
using MedLink.Presenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Web.Controllers;

[Authorize]
public class AppointmentController : Controller
{
    private readonly AppointmentPresenter _presenter;
    private readonly DoctorListPresenter _doctorPresenter;
    private readonly IPatientRepository _patients;
    private readonly ReviewPresenter _reviewPresenter;
    private readonly AppDbContext _db;

    public AppointmentController(AppointmentPresenter presenter, DoctorListPresenter doctorPresenter,
        IPatientRepository patients, ReviewPresenter reviewPresenter, AppDbContext db)
    {
        _presenter = presenter;
        _doctorPresenter = doctorPresenter;
        _patients = patients;
        _reviewPresenter = reviewPresenter;
        _db = db;
    }

    private string UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    public async Task<IActionResult> Index()
    {
        var appointments = await _presenter.GetPatientAppointmentsAsync(UserId);
        return View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> Book(int doctorId)
    {
        var doctor = await _doctorPresenter.GetDoctorDetailAsync(doctorId);
        if (doctor == null) return NotFound();

        var patient = await _patients.GetByUserIdAsync(UserId);
        if (patient == null)
        {
            TempData["Warning"] = "Please complete your profile before booking an appointment.";
            return RedirectToAction("Profile", "Account");
        }

        ViewBag.Doctor = doctor;
        var model = new BookAppointmentViewModel
        {
            DoctorId = doctorId,
            DoctorName = doctor.Name,
            DoctorSpecialty = doctor.SpecialtyName,
            Fee = doctor.ConsultationFee
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Book(BookAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var doctor = await _doctorPresenter.GetDoctorDetailAsync(model.DoctorId);
            ViewBag.Doctor = doctor;
            return View(model);
        }

        var (success, message, apptId) = await _presenter.BookAsync(model, UserId);
        if (!success)
        {
            ModelState.AddModelError("", message);
            var doctor = await _doctorPresenter.GetDoctorDetailAsync(model.DoctorId);
            ViewBag.Doctor = doctor;
            return View(model);
        }

        TempData["Success"] = message;
        return RedirectToAction("Confirmation", new { id = apptId });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var appointments = await _presenter.GetPatientAppointmentsAsync(UserId);
        var appt = appointments.FirstOrDefault(a => a.Id == id);
        if (appt == null) return NotFound();
        return View(appt);
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var success = await _presenter.CancelAsync(id, UserId);
        TempData[success ? "Success" : "Error"] = success ? "Appointment cancelled." : "Unable to cancel this appointment.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> HealthRecord(int id)
    {
        var record = await _presenter.GetHealthRecordAsync(id, UserId);
        if (record == null) return NotFound();
        return View(record);
    }

    [HttpPost]
    public async Task<IActionResult> Review(SubmitReviewViewModel vm)
    {
        var (success, message) = await _reviewPresenter.SubmitAsync(vm, UserId);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction("Index");
    }

    // AJAX endpoint — return available slots for a day
    [HttpGet]
    public async Task<IActionResult> GetSlots(int doctorId, string date)
    {
        if (!DateTime.TryParse(date, out var parsed)) return BadRequest();
        var dayOfWeek = parsed.DayOfWeek;
        var slots = await _db.TimeSlots
            .Where(t => t.DoctorId == doctorId && t.DayOfWeek == dayOfWeek && !t.IsBooked)
            .OrderBy(t => t.StartTime)
            .Select(t => new { t.Id, display = $"{t.StartTime:hh\\:mm} - {t.EndTime:hh\\:mm}" })
            .ToListAsync();
        return Json(slots);
    }
}