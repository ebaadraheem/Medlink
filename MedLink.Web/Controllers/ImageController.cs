using MedLink.Model.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Web.Controllers;

public class ImageController : Controller
{
    private readonly AppDbContext _db;
    public ImageController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Doctor(int id)
    {
        var doctor = await _db.Doctors.Where(d => d.Id == id).Select(d => new { d.PhotoData, d.PhotoContentType }).FirstOrDefaultAsync();
        if (doctor?.PhotoData == null) return NotFound();
        return File(doctor.PhotoData, doctor.PhotoContentType ?? "image/jpeg");
    }
}