using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SweetShop.API.Data;
using SweetShop.API.Models;
using System.Net.Http;

namespace SweetShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // JWT required
    public class SweetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SweetsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/sweets
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sweets = await _context.Sweets
                .Where(s => !s.IsDeleted)
                .ToListAsync();
            return Ok(sweets);
        }

        // GET: api/sweets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sweet = await _context.Sweets.FindAsync(id);
            if (sweet == null || sweet.IsDeleted) return NotFound();
            return Ok(sweet);
        }

        // POST: api/sweets
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSweet([FromForm] Sweet sweet)
        {
            sweet.CreatedAt = DateTime.UtcNow;
            sweet.UpdatedAt = DateTime.UtcNow;
            sweet.IsDeleted = false;

            _context.Sweets.Add(sweet);
            await _context.SaveChangesAsync();
            return Ok(sweet);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSweet(int id, [FromForm] Sweet sweet, IFormFile? ImageFile)
        {
            var existingSweet = await _context.Sweets.FindAsync(id);
            if (existingSweet == null || existingSweet.IsDeleted)
                return NotFound("Sweet not found.");

            existingSweet.Name = sweet.Name;
            existingSweet.Category = sweet.Category;
            existingSweet.Price = sweet.Price;
            existingSweet.Quantity = sweet.Quantity;
            existingSweet.Description = sweet.Description;

            // Only update image if new file uploaded
            if (ImageFile != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "Images", "Sweets");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await ImageFile.CopyToAsync(stream);

                existingSweet.ImageUrl = $"/Images/Sweets/{fileName}";
            }
            // If ImageFile null, keep existing ImageUrl intact

            existingSweet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(existingSweet);
        }

        // DELETE: api/sweets/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSweet(int id)
        {
            var exitingSweet = await _context.Sweets.FindAsync(id);
            if (exitingSweet == null || exitingSweet.IsDeleted)
                return NotFound("Sweet not found.");

            exitingSweet.IsDeleted = true;
            exitingSweet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok($"{exitingSweet.Name} deleted successfully.");
        }

        // POST: api/sweets/{id}/purchase
        [HttpPost("{id}/purchase")]
        public async Task<IActionResult> PurchaseSweet(int id)
        {
            var exitingSweet = await _context.Sweets.FindAsync(id);
            if (exitingSweet == null || exitingSweet.IsDeleted)
                return NotFound("Sweet not found.");

            if (exitingSweet.Quantity <= 0)
                return BadRequest($"{exitingSweet.Name} is out of stock.");

            exitingSweet.Quantity -= 1;
            exitingSweet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok($"Purchase of {exitingSweet.Name} successful.");
        }

        // POST: api/sweets/{id}/restock
        [HttpPost("{id}/restock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestockSweet(int id, [FromQuery] int quantity)
        {
            var exitingSweet = await _context.Sweets.FindAsync(id);
            if (exitingSweet == null || exitingSweet.IsDeleted)
                return NotFound("Sweet not found.");

            exitingSweet.Quantity += quantity;
            exitingSweet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(exitingSweet);
        }
    }
}
