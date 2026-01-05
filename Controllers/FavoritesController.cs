using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vinted2.Data;
using vinted2.Models;

namespace vinted2.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Favorites
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            try
            {
                var favorites = await _context.Favorites
                    .Where(f => f.UserId == user.Id)
                    .Include(f => f.Product)
                        .ThenInclude(p => p.Category)
                    .Include(f => f.Product)
                        .ThenInclude(p => p.Images)
                    .Include(f => f.Product)
                        .ThenInclude(p => p.User)
                    .OrderByDescending(f => f.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                // DEBUG - șterge după ce merge
                Console.WriteLine($"Found {favorites.Count} favorites for user {user.Id}");
                foreach (var fav in favorites)
                {
                    Console.WriteLine($"Favorite: ProductId={fav.ProductId}, Product is null: {fav.Product == null}");
                }

                return View(favorites);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Favorites Index: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return View(new List<Favorite>());
            }
        }

        // POST: Favorites/Toggle
        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] ToggleRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Verifică dacă produsul există
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                // Verifică dacă există deja în favorites
                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == request.ProductId);

                if (favorite != null)
                {
                    // Șterge din favorites
                    _context.Favorites.Remove(favorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorite = false, message = "Removed from favorites" });
                }
                else
                {
                    // Adaugă la favorites
                    var newFavorite = new Favorite
                    {
                        UserId = user.Id,
                        ProductId = request.ProductId,
                        CreatedAt = DateTime.Now
                    };

                    _context.Favorites.Add(newFavorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorite = true, message = "Added to favorites" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Favorites/Remove
        [HttpPost]
        public async Task<IActionResult> Remove([FromBody] ToggleRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == request.ProductId);

                if (favorite == null)
                {
                    return Json(new { success = false, message = "Not in favorites" });
                }

                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Removed from favorites" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Check if product is favorited
        [HttpGet]
        public async Task<IActionResult> Check(int productId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { isFavorite = false });
                }

                var isFavorite = await _context.Favorites
                    .AnyAsync(f => f.UserId == user.Id && f.ProductId == productId);

                return Json(new { isFavorite });
            }
            catch (Exception ex)
            {
                return Json(new { isFavorite = false, error = ex.Message });
            }
        }
    }

    // Model pentru request
    public class ToggleRequest
    {
        public int ProductId { get; set; }
    }
}