using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using System.Text.RegularExpressions;

namespace Discord2.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(ApplicationDbContext context, 
                                UserManager<AppUser> userManager,
                                RoleManager<IdentityRole> roleManager) 
        {
            _userManager = userManager;
            _roleManager = roleManager;
            db = context;
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Fetch groups the user is part of
            var groupsQuery = db.Groups
                .Where(g => g.Memberships.Any(m => m.UserId == userId));

            var searchTerm= HttpContext.Request.Query["search"].FirstOrDefault()?.Trim();
            // Filter groups by search term if entered
            if (!string.IsNullOrEmpty(searchTerm))
            {
                groupsQuery = groupsQuery.Where(g => g.Name.Contains(searchTerm));
            }

            var groups = await groupsQuery.ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            return View(groups);
        }
        [Authorize(Roles = "Admin,User")]
        public IActionResult Show(int id)
        {
            var userId = _userManager.GetUserId(User);
            bool isUserInGroup = db.Memberships
                                .Any(m => m.UserId == userId && m.GroupId == id);
            if (isUserInGroup || User.IsInRole("Admin"))
            {

                var data = (from g in db.Groups.Include(g => g.Channels).Include("Channels.Category")
                            where g.Id == id
                            select g).ToList();
                return View(data);
            } else
            {
                TempData["message"] = "You need to join the group to see its contents";
                return RedirectToAction("Index", "Home");
            }
        }
        [Authorize(Roles ="User,Admin")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Group g)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var roleId = (from role in db.GroupRoles
                             where role.Name == "Admin"
                             select role.Id).First();
                     db.Groups.Add(g);
                db.SaveChanges();
                var membership = new Membership
                {
                    GroupId = g.Id,
                    UserId = userId,
                    GroupRoleId = roleId
                };
                db.Memberships.Add(membership);
                db.SaveChanges();
                return RedirectToAction("Index");
            } else
            {
                return View();
            }
        }
    }
}
