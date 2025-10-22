using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace WebGiayOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductGroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductGroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ProductGroup/Index
        public async Task<IActionResult> Index()
        {
            var groups = await _context.ProductGroups
                .Include(pg => pg.Variants)  // Load biến thể kèm theo
                .ToListAsync();

            return View(groups);
        }

        // GET: /ProductGroup/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var group = await _context.ProductGroups
                .Include(pg => pg.Variants)
                    .ThenInclude(g => g.ProductImages)
                .FirstOrDefaultAsync(pg => pg.ProductGroupId == id);

            if (group == null)
                return NotFound();

            return View(group);
        }

        public async Task<IActionResult> DisplayGroup(int productGroupId)
        {
            var productGroup = await _context.ProductGroups
                .Include(pg => pg.Variants)
                    .ThenInclude(g => g.ProductImages)
                .FirstOrDefaultAsync(pg => pg.ProductGroupId == productGroupId);

            if (productGroup == null)
                return NotFound();

            return View(productGroup);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductGroup group)
        {
            if (ModelState.IsValid)
            {
                _context.Add(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // GET: ProductGroup/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var group = await _context.ProductGroups.FindAsync(id);
            if (group == null) return NotFound();

            return View(group);
        }

        // POST: ProductGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductGroup group)
        {
            if (id != group.ProductGroupId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductGroupExists(group.ProductGroupId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // GET: ProductGroup/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var group = await _context.ProductGroups
                .FirstOrDefaultAsync(pg => pg.ProductGroupId == id);
            if (group == null) return NotFound();

            return View(group);
        }

        // POST: ProductGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.ProductGroups.FindAsync(id);
            if (group != null)
            {
                _context.ProductGroups.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductGroupExists(int id)
        {
            return _context.ProductGroups.Any(e => e.ProductGroupId == id);
        }
    }
}

