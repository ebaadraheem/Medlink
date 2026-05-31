using MedLink.Model.Data;
using MedLink.Model.Entities;
using MedLink.Model.Repositories;
using MedLink.Presenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedLink.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IPatientRepository _patients;
    private readonly AppDbContext _db;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
        IPatientRepository patients, AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _patients = patients;
        _db = db;
    }

    private string UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    public async Task<IActionResult> Profile()
    {
        var patient = await _patients.GetByUserIdAsync(UserId);
        if (patient == null)
            return View(new PatientProfileViewModel { DateOfBirth = DateTime.Today.AddYears(-20) });

        var vm = new PatientProfileViewModel
        {
            FullName = patient.FullName,
            StudentId = patient.StudentId,
            Department = patient.Department,
            Phone = patient.Phone,
            DateOfBirth = patient.DateOfBirth,
            BloodGroup = patient.BloodGroup,
            EmergencyContact = patient.EmergencyContact,
            MedicalHistory = patient.MedicalHistory
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Profile(PatientProfileViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var patient = await _patients.GetByUserIdAsync(UserId);
        if (patient == null)
        {
            patient = new Patient { UserId = UserId };
            await _patients.AddAsync(patient);
        }

        patient.FullName = vm.FullName;
        patient.StudentId = vm.StudentId;
        patient.Department = vm.Department;
        patient.Phone = vm.Phone;
        patient.DateOfBirth = DateTime.SpecifyKind(vm.DateOfBirth, DateTimeKind.Utc);
        patient.BloodGroup = vm.BloodGroup;
        patient.EmergencyContact = vm.EmergencyContact;
        patient.MedicalHistory = vm.MedicalHistory;
        patient.UpdatedAt = DateTime.UtcNow;

        await _patients.SaveChangesAsync();
        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction("Profile");
    }
}