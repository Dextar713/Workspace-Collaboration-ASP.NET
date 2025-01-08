using Discord2.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discord2.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult UserList()
        {
            var users = _db.Users.ToList();
            return View(users);
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            var user = _db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _db.Users.Remove(user);
            _db.SaveChanges();
            TempData["message"] = "User deleted successfully.";
            return RedirectToAction("UserList");
        }
    }
}
