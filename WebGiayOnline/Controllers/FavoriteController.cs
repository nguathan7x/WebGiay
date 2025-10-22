using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;
using WebGiayOnline.Models;

[Authorize]
public class FavoriteController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoriteController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int giayId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.GiayId == giayId && f.UserId == userId);

        if (favorite == null)
        {
            favorite = new Favorite { GiayId = giayId, UserId = userId };
            _context.Favorites.Add(favorite);
        }
        else
        {
            _context.Favorites.Remove(favorite);
        }

        await _context.SaveChangesAsync();

        return Ok(new { isFavorite = favorite != null });
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Challenge(); // hoặc RedirectToAction("Login")

        // Lấy danh sách sản phẩm yêu thích kèm thông tin sản phẩm
        var favoriteProducts = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Giay)    // Bao gồm thông tin giày
            .ThenInclude(g => g.Brand)   // Nếu muốn thêm thông tin brand
            .ToListAsync();

        return View(favoriteProducts);
    }
}

