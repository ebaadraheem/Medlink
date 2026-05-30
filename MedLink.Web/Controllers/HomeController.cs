using MedLink.Presenter.Presenters;
using Microsoft.AspNetCore.Mvc;

namespace MedLink.Web.Controllers;

public class HomeController : Controller
{
    private readonly DoctorListPresenter _presenter;

    public HomeController(DoctorListPresenter presenter)
    {
        _presenter = presenter;
    }

    public async Task<IActionResult> Index()
    {
        var specialties = await _presenter.GetSpecialtiesAsync();
        var doctors = await _presenter.GetDoctorsAsync(null, null, "rating");
        ViewBag.Specialties = specialties;
        ViewBag.FeaturedDoctors = doctors.Take(6).ToList();
        return View();
    }

    public async Task<IActionResult> Doctors(string? search, int? specialtyId, string? sort)
    {
        var doctors = await _presenter.GetDoctorsAsync(search, specialtyId, sort);
        var specialties = await _presenter.GetSpecialtiesAsync();
        ViewBag.Specialties = specialties;
        ViewBag.Search = search;
        ViewBag.SelectedSpecialty = specialtyId;
        ViewBag.Sort = sort;
        return View(doctors);
    }

    public async Task<IActionResult> DoctorDetail(int id)
    {
        var doctor = await _presenter.GetDoctorDetailAsync(id);
        if (doctor == null) return NotFound();
        ViewBag.CanBook = User.Identity?.IsAuthenticated ?? false;
        return View(doctor);
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
    public IActionResult Terms() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}