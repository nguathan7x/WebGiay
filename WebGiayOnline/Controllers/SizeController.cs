using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using System.Threading.Tasks;

namespace WebGiayOnline.Controllers
{
    public class SizeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SizeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Size
        public async Task<IActionResult> Index()
        {
            var sizes = (await _context.Sizes.ToListAsync())
                .OrderBy(s => ConvertLabelToNumber(s.Label))
                .ToList();

            return View(sizes);
        }

        private double ConvertLabelToNumber(string label)
        {
            var numberPart = new string(label.Where(c => char.IsDigit(c) || c == '.').ToArray());

            if (double.TryParse(numberPart, out var number))
            {
                return number;
            }

            return double.MaxValue;
        }



        // GET: Size/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Size/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Size size)
        {
            if (ModelState.IsValid)
            {
                _context.Add(size);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(size);
        }

        // GET: Size/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var size = await _context.Sizes.FindAsync(id);
            if (size == null) return NotFound();

            return View(size);
        }

        // POST: Size/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Size size)
        {
            if (id != size.SizeId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(size);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await SizeExists(size.SizeId))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(size);
        }

        // GET: Size/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var size = await _context.Sizes
                .FirstOrDefaultAsync(m => m.SizeId == id);
            if (size == null) return NotFound();

            return View(size);
        }

        // POST: Size/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var size = await _context.Sizes.FindAsync(id);
            if (size != null)
            {
                _context.Sizes.Remove(size);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> SizeExists(int id)
        {
            return await _context.Sizes.AnyAsync(e => e.SizeId == id);
        }
    }
}
