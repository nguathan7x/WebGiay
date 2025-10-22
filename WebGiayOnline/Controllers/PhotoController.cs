using Microsoft.AspNetCore.Mvc;

public class PhotoController : Controller
{
    private readonly PhotoService _photoService;

    public PhotoController(PhotoService photoService)
    {
        _photoService = photoService;
    }

    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
        {
            ModelState.AddModelError("photo", "Vui lòng chọn ảnh.");
            return View();
        }

        var result = await _photoService.UploadPhotoAsync(photo);
        ViewBag.ImageUrl = result.SecureUrl.ToString();
        return View();
    }
}
