using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using vinted2.Data;
using vinted2.Models;
using vinted2.ViewModels;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Checkout(int productId)
    {
        var model = new CheckoutViewModel
        {
            ProductId = productId
        };

        return View(model);
    }

    // 📦 Place order
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Checkout", model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == model.ProductId);

        if (product == null)
            return NotFound();

        decimal price = product.Price > 0 ? product.Price : 0;

        var order = new Order
        {
            ProductId = product.Id,
            UserId = user.Id,

            FullName = model.FullName,
            Address = model.Address,
            City = model.City,
            Phone = model.Phone,
            DeliveryMethod = model.DeliveryMethod,

            Price = price,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.Now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return RedirectToAction("Success");
    }

    public IActionResult Success()
    {
        return View();
    }

    public async Task<IActionResult> MyOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var orders = await _context.Orders
            .Include(o => o.Product!)
                .ThenInclude(p => p.Images)
            .Include(o => o.Product!)
                .ThenInclude(p => p.User)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> ManageOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var orders = await _context.Orders
            .Include(o => o.Product!)
                .ThenInclude(p => p.Images)
            .Include(o => o.User)
            .Where(o => o.Product != null && o.Product.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsSent(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var order = await _context.Orders
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null || order.Product == null || order.Product.UserId != user.Id)
            return NotFound();

        if (order.Status != OrderStatus.Pending)
            return BadRequest("Order can only be marked as sent when status is Pending.");

        order.Status = OrderStatus.InTransit;
        await _context.SaveChangesAsync();

        return RedirectToAction("ManageOrders");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var order = await _context.Orders
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null || order.Product == null || order.Product.UserId != user.Id)
            return NotFound();

        if (order.Status != OrderStatus.Pending)
            return BadRequest("Order can only be canceled when status is Pending.");

        order.Status = OrderStatus.Canceled;
        await _context.SaveChangesAsync();

        return RedirectToAction("ManageOrders");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsReceived(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null || order.UserId != user.Id)
            return NotFound();

        if (order.Status != OrderStatus.InTransit)
            return BadRequest("Order can only be marked as received when status is InTransit.");

        order.Status = OrderStatus.Completed;
        await _context.SaveChangesAsync();

        return RedirectToAction("MyOrders");
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var order = await _context.Orders
            .Include(o => o.Product!)
                .ThenInclude(p => p.Images)
            .Include(o => o.Product!)
                .ThenInclude(p => p.User)
            .Include(o => o.Product!)
                .ThenInclude(p => p.Category)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        // Check if user is either the buyer or the seller
        bool isBuyer = order.UserId == user.Id;
        bool isSeller = order.Product != null && order.Product.UserId == user.Id;

        if (!isBuyer && !isSeller)
            return Forbid();

        ViewBag.IsBuyer = isBuyer;
        ViewBag.IsSeller = isSeller;

        return View(order);
    }
}
