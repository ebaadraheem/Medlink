using MedLink.Model.Data;
using MedLink.Model.Enums;
using MedLink.Presenter.Presenters;
using MedLink.Presenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Web.Controllers.Admin;

[Authorize(Policy = "AdminOnly")]
[Route("admin/[controller]/[action]")]
public class DashboardController : Controller
{
    private readonly AdminDashboardPresenter _presenter;
    public DashboardController(AdminDashboardPresenter presenter) => _presenter = presenter;

    [Route("/admin"), Route("/admin/dashboard")]
    public async Task<IActionResult> Index()
        => View(await _presenter.GetDashboardAsync());
}

[Authorize(Policy = "AdminOnly")]
[Route("admin/doctors")]
public class AdminDoctorsController : Controller
{
    private readonly AdminDoctorPresenter _presenter;
    public AdminDoctorsController(AdminDoctorPresenter presenter) => _presenter = presenter;

    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _presenter.GetAllAsync());
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id) 
    {
        return View(await _presenter.GetForEditAsync(id));
    }
    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var vm = new AdminDoctorViewModel { Specialties = await _presenter.GetSpecialtyListAsync() };
        return View(vm);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(AdminDoctorViewModel vm)
    {
        // FORCE the system to ignore validation on these fields
        ModelState.Remove("Specialties"); 
        ModelState.Remove("PhotoUpload");
        if (string.IsNullOrEmpty(vm.Phone)) ModelState.Remove("Phone");
        if (string.IsNullOrEmpty(vm.Email)) ModelState.Remove("Email");

        if (!ModelState.IsValid)
        {
            // If it fails, reload the dropdowns and show the form again
            vm.Specialties = await _presenter.GetSpecialtyListAsync();
            return View(vm);
        }

        byte[]? photoData = null;
        string? contentType = null;
        
        // Grab the file directly from the raw HTML form
        var uploadedFile = Request.Form.Files.FirstOrDefault();
        
        if (uploadedFile != null && uploadedFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await uploadedFile.CopyToAsync(ms);
            photoData = ms.ToArray();
            contentType = uploadedFile.ContentType;
        }

        await _presenter.CreateAsync(vm, photoData, contentType);
        TempData["Success"] = "Doctor added successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(int id, AdminDoctorViewModel vm) 
    {
        vm.Id = id; 
        
        // FORCE the system to ignore validation on these fields
        ModelState.Remove("Specialties"); 
        ModelState.Remove("PhotoUpload");
        if (string.IsNullOrEmpty(vm.Phone)) ModelState.Remove("Phone");
        if (string.IsNullOrEmpty(vm.Email)) ModelState.Remove("Email");

        if (!ModelState.IsValid)
        {
            // If it still fails, it's because a Required field like Name or Bio is empty
            vm.Specialties = await _presenter.GetSpecialtyListAsync();
            return View(vm);
        }
        
        byte[]? photoData = null; 
        string? contentType = null;
        
        var uploadedFile = Request.Form.Files.FirstOrDefault();
        if (uploadedFile != null && uploadedFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await uploadedFile.CopyToAsync(ms);
            photoData = ms.ToArray(); 
            contentType = uploadedFile.ContentType;
        }
        
        // This is where the database actually gets updated!
        await _presenter.UpdateAsync(vm, photoData, contentType);
        
        TempData["Success"] = "Doctor updated.";
        return RedirectToAction("Index");
    }

    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _presenter.DeleteAsync(id);
        TempData["Success"] = "Doctor removed.";
        return RedirectToAction("Index");
    }
}

[Authorize(Policy = "AdminOnly")]
[Route("admin/appointments")]
public class AdminAppointmentsController : Controller
{
    private readonly AdminAppointmentPresenter _presenter;
    public AdminAppointmentsController(AdminAppointmentPresenter presenter) => _presenter = presenter;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? status)
    {
        ViewBag.StatusFilter = status;
        return View(await _presenter.GetAllAsync(status));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var appt = await _presenter.GetDetailAsync(id);
        if (appt == null) return NotFound();
        return View(appt);
    }

    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? notes, string? diagnosis, string? prescription, decimal? weight, decimal? height, string? bloodPressure, decimal? temperature)
    {
        if (Enum.TryParse<AppointmentStatus>(status, out var s))
            await _presenter.UpdateStatusAsync(id, s, notes, diagnosis, prescription, weight, height, bloodPressure, temperature);
        
        TempData["Success"] = "Appointment updated.";
        return RedirectToAction("Detail", new { id });
    }
}

[Authorize(Policy = "AdminOnly")]
[Route("admin/patients")]
public class AdminPatientsController : Controller
{
    private readonly AppDbContext _db;
    public AdminPatientsController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var patients = await _db.Patients
            .Include(p => p.Appointments)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(patients);
    }
}

[Authorize(Policy = "AdminOnly")]
[Route("admin/specialties")]
public class AdminSpecialtiesController : Controller
{
    private readonly AppDbContext _db;
    public AdminSpecialtiesController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Specialties.Include(s => s.Doctors).ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new SpecialtyViewModel());

    [HttpPost("create")]
    public async Task<IActionResult> Create(SpecialtyViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await _db.Specialties.AddAsync(new MedLink.Model.Entities.Specialty { Name = vm.Name, Description = vm.Description, Icon = vm.Icon });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Specialty created.";
        return RedirectToAction("Index");
    }
}