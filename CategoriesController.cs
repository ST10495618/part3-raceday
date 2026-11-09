using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;
using RaceDay.Models;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : Controller
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/1
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Category name is required." });

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                category);
        }

        // DELETE: api/categories/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            bool hasEvents = await _context.Events
                .AnyAsync(e => e.CategoryID == id);

            if (hasEvents)
            {
                return Conflict(new
                {
                    message = "This category is used by events and cannot be deleted."
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }

        // PUT: api/categories/1
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id, Category updatedCategory)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (string.IsNullOrWhiteSpace(updatedCategory.Name))
                return BadRequest(new
                {
                    message = "Category name is required."
                });

            category.Name = updatedCategory.Name.Trim();
            category.Description = updatedCategory.Description;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully.",
                category.CategoryID,
                category.Name,
                category.Description
            });
        }

    }
}
