using MedLink.Model.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // Required for IMemoryCache

namespace MedLink.Web.Controllers;

public class ImageController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache; // Our RAM Cache

    // Inject the cache via the constructor
    public ImageController(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    [Route("Image/Doctor/{id}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)] // Tell the user's browser to cache this for 24 hours
    public async Task<IActionResult> Doctor(int id)
    {
        var photoCacheKey = $"DoctorPhoto_{id}";
        var typeCacheKey = $"DoctorPhotoType_{id}";

        // 1. Try to get the image from the web server's incredibly fast RAM
        if (!_cache.TryGetValue(photoCacheKey, out byte[]? photoData))
        {
            // 2. If it's NOT in RAM, fetch it from Supabase (this only happens once!)
            var doctor = await _db.Doctors
                .Where(d => d.Id == id)
                .Select(d => new { d.PhotoData, d.PhotoContentType })
                .FirstOrDefaultAsync();

            if (doctor?.PhotoData == null) return NotFound();

            photoData = doctor.PhotoData;
            var contentType = doctor.PhotoContentType ?? "image/jpeg";

            // 3. Save it in RAM for 2 hours so Supabase gets a break
            var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(2));
            _cache.Set(photoCacheKey, photoData, cacheOptions);
            _cache.Set(typeCacheKey, contentType, cacheOptions);

            return File(photoData, contentType);
        }

        // 4. Return the instantly loaded image from RAM
        var cachedType = _cache.Get<string>(typeCacheKey) ?? "image/jpeg";
        return File(photoData, cachedType);
    }
}