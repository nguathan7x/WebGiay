using Microsoft.AspNetCore.Mvc;
using WebGiayOnline.Models; // Sửa nếu namespace của bạn khác
using WebGiayOnline.Data;   // DbContext
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

public class FavoriteShoesViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public FavoriteShoesViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var favorites = await _context.Favorites
            .Include(f => f.Giay)
            .ToListAsync();

        return View(favorites);
    }
}
