using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using vinted2.Data;
using vinted2.Models;

namespace vinted2.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string? category)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.Images);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && 
                    p.Category.Name.ToLower() == category.ToLower());
            }

            var products = await query.ToListAsync();
            
            ViewData["CurrentCategory"] = category;
            ViewData["Title"] = string.IsNullOrEmpty(category) ? "All Products" : $"{char.ToUpper(category[0]) + category.Substring(1)} Products";
            
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,ImagePath,CategoryId")] Product product, IFormFileCollection images)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    product.UserId = await GetOrCreateDefaultUser();
                }
                else
                {
                    product.UserId = userId;
                }
                product.CreatedAt = DateTime.Now;

                _context.Add(product);
                await _context.SaveChangesAsync();

                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        if (image.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            var filePath = Path.Combine("wwwroot", "images", "products", fileName);
                            
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                            
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            var productImage = new ProductImage
                            {
                                ProductId = product.Id,
                                ImagePath = $"/images/products/{fileName}",
                                CreatedAt = DateTime.Now
                            };

                            _context.ProductImages.Add(productImage);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }

            if (!CanEditProduct(product))
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,ImagePath,CategoryId,UserId,CreatedAt")] Product product, IFormFileCollection images)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var originalProduct = await _context.Products.FindAsync(id);
            if (originalProduct == null)
            {
                return NotFound();
            }

            if (!CanEditProduct(originalProduct))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {  
                    originalProduct.Title = product.Title;
                    originalProduct.Description = product.Description;
                    originalProduct.Price = product.Price;
                    originalProduct.ImagePath = product.ImagePath;
                    originalProduct.CategoryId = product.CategoryId;

                    if (images != null && images.Count > 0)
                    {
                        foreach (var image in images)
                        {
                            if (image.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                                var filePath = Path.Combine("wwwroot", "images", "products", fileName);
                                
                                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                                
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(stream);
                                }

                                var productImage = new ProductImage
                                {
                                    ProductId = originalProduct.Id,
                                    ImagePath = $"/images/products/{fileName}",
                                    CreatedAt = DateTime.Now
                                };

                                _context.ProductImages.Add(productImage);
                            }
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", product.UserId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (!CanEditProduct(product))
            {
                return Forbid();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }

            if (!CanEditProduct(product))
            {
                return Forbid();
            }

            // Delete all related ProductImages
            if (product.Images != null && product.Images.Any())
            {
                _context.ProductImages.RemoveRange(product.Images);
            }

            // Delete all related Favorites
            if (product.Favorites != null && product.Favorites.Any())
            {
                _context.Favorites.RemoveRange(product.Favorites);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        private bool CanEditProduct(Product product)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (product.UserId == currentUserId)
            {
                return true;
            }

            if (User.IsInRole("Admin"))
            {
                return true;
            }

            return false;
        }

        private async Task<string> GetOrCreateDefaultUser()
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return existingUser.Id;
            }

            var defaultUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "default@example.com",
                Email = "default@example.com",
                EmailConfirmed = true,
                NormalizedUserName = "DEFAULT@EXAMPLE.COM",
                NormalizedEmail = "DEFAULT@EXAMPLE.COM",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            _context.Users.Add(defaultUser);
            await _context.SaveChangesAsync();
            return defaultUser.Id;
        }
    }
}
