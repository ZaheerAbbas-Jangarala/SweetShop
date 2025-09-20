using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SweetShop.Mvc.Models;
using SweetShop.Mvc.Services;
using System.Data;

namespace SweetShop.Mvc.Controllers
{
    public class SweetsController : Controller
    {
        private readonly SweetService _sweetService;
        private readonly IWebHostEnvironment _env;

        public SweetsController(SweetService sweetService, IWebHostEnvironment env)
        {
            _sweetService = sweetService;
            _env = env;
        }

        private List<string> GetCategoryList() => new()
        {
            "Indian Sweets",
            "Western Sweets",
            "Middle Eastern Sweets",
            "East Asian Sweets",
            "European Desserts",
            "Fusion / Modern Sweets"
        };

        // ---------- USER ----------
        public async Task<IActionResult> Shop(string? q, string? category, decimal? minPrice, decimal? maxPrice)
        {
            var sweets = await _sweetService.GetAllSweetsAsync();

            if (!string.IsNullOrWhiteSpace(q))
                sweets = sweets.Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(category))
                sweets = sweets.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

            if (minPrice.HasValue)
                sweets = sweets.Where(s => s.Price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                sweets = sweets.Where(s => s.Price <= maxPrice.Value).ToList();

            ViewData["SearchQuery"] = q;
            ViewData["SelectedCategory"] = category;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;

            return View(sweets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int id)
        {
            var ok = await _sweetService.PurchaseAsync(id);
            TempData["Message"] = ok ? "Purchase successful." : "Purchase failed.";
            return RedirectToAction(nameof(Shop));
        }

        // ---------- ADMIN ----------
        public async Task<IActionResult> AdminIndex(string? q)
        {
            var sweets = await _sweetService.GetAllSweetsAsync();

            if (!string.IsNullOrWhiteSpace(q))
            {
                sweets = sweets
                    .Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                             || s.Category.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewData["SearchQuery"] = q;
            return View(sweets);
        }


        public IActionResult Create()
        {
            ViewBag.Categories = GetCategoryList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sweet sweet)
        {
            if (ModelState.IsValid)
            {
                if (sweet.ImageFile != null)
                {
                    var folder = Path.Combine(_env.WebRootPath, "Images", "Sweets");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(sweet.ImageFile.FileName);
                    var path = Path.Combine(folder, fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await sweet.ImageFile.CopyToAsync(stream);

                    sweet.ImageUrl = $"/Images/Sweets/{fileName}";
                }

                var r = await _sweetService.AddSweetAsync(sweet);
                if (r) return RedirectToAction(nameof(AdminIndex));

                ModelState.AddModelError("", "Failed to add sweet.");
            }

            ViewBag.Categories = GetCategoryList();
            return View(sweet);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sweet = await _sweetService.GetSweetByIdAsync(id);
            if (sweet == null) return NotFound();

            ViewBag.Categories = GetCategoryList();
            return View(sweet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Sweet sweet)
        {
            if (ModelState.IsValid)
            {
                // Image update logic
                if (sweet.ImageFile != null)
                {
                    var folder = Path.Combine(_env.WebRootPath, "Images", "Sweets");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(sweet.ImageFile.FileName);
                    var path = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await sweet.ImageFile.CopyToAsync(stream);
                    }

                    sweet.ImageUrl = "/Images/Sweets/" + fileName;
                }
                // If ImageFile is null, keep existing ImageUrl intact (already in sweet.ImageUrl)

                // Call service
                var r = await _sweetService.UpdateSweetAsync(sweet);
                if (r) return RedirectToAction(nameof(AdminIndex));

                ModelState.AddModelError("", "Failed to update sweet.");
            }

            ViewBag.Categories = GetCategoryList();
            return View(sweet);
        }

        public async Task<IActionResult> Details(int id)
        {
            var sweet = await _sweetService.GetSweetByIdAsync(id);
            if (sweet == null) return NotFound();
            return View(sweet);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var sweet = await _sweetService.GetSweetByIdAsync(id);
            if (sweet == null) return NotFound();
            return View(sweet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _sweetService.DeleteSweetAsync(id);
            return RedirectToAction(nameof(AdminIndex));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restock(int id, int addQuantity)
        {
            var r = await _sweetService.RestockAsync(id, addQuantity);
            TempData["Message"] = r ? "Restocked successfully" : "Restock failed";
            return RedirectToAction(nameof(AdminIndex));
        }
    }
}
