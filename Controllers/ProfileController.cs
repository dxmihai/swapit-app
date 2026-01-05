using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using vinted2.Models;
using vinted2.ViewModels;

namespace vinted2.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        // GET: /Profile/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            if (string.IsNullOrEmpty(user.AvatarPath))
            {
                user.AvatarPath = "/images/profiles/default-avatar.png";
                await _userManager.UpdateAsync(user);
            }

            var model = new ProfileViewModel
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                AvatarPath = user.AvatarPath
            };

            return View(model);
        }

        // POST: /Profile/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (user.Email != model.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
            }

            user.DisplayName = model.DisplayName;

            if (!string.IsNullOrEmpty(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, model.Password);
            }

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "profiles");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.AvatarPath = "/images/profiles/" + uniqueFileName;
            }
            else if (string.IsNullOrEmpty(user.AvatarPath))
            {
                user.AvatarPath = "/images/profiles/default-avatar.png";
            }

            await _userManager.UpdateAsync(user);

            model.AvatarPath = user.AvatarPath;
            ViewBag.Message = "Profile updated successfully!";

            return View(model);
        }
    }
}
