using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discord2.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext db;

        public ProfileController(UserManager<AppUser> userManager,
                                ApplicationDbContext context)
        {
            _userManager = userManager;
            db = context;
        }

        [Authorize(Roles = "Admin,User,Moderator")]
        public async Task<IActionResult> Edit()
        {
            UpdateUserStats();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var model = new Profile
            {
                UserName = user.UserName,
                Email = user.Email,
                Bio = user.Bio,
                CurrentAvatarPath = user.AvatarPath
            };
            ViewBag.CurUser = user;
            return View(model);
        }

        [Authorize(Roles ="Admin,User,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Edit(Profile model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Bio = model.Bio;

                if (model.Avatar != null)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "uploads", "avatars");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the folder exists
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Avatar.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Avatar.CopyToAsync(stream);
                    }

                    // Delete the old avatar if it exists
                    if (!string.IsNullOrEmpty(user.AvatarPath))
                    {
                        var oldFilePath = Path.Combine("wwwroot", user.AvatarPath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    user.AvatarPath = Path.Combine("uploads", "avatars", uniqueFileName).Replace("\\", "/");
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["message"] = "Profile updated successfully.";
                    return RedirectToAction("Edit");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.CurUser = user;
            UpdateUserStats();
            return View(model);
        }

        [NonAction]
        private void UpdateUserStats()
        {
            var user = _userManager.GetUserAsync(User).Result;
            user.TotalGroups = db.Memberships.Where(m => m.UserId == user.Id).Count();
            user.DaysActive = db.Messages
                                .Where(m => m.UserId == user.Id) 
                                .Select(m => m.DateTime.Date) 
                                .Distinct()                    
                                .Count();
        }
    }

}
