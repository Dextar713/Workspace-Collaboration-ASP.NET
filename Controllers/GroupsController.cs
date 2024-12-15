using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

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

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            // Fetch groups the user is part of
            var userGroups = db.Groups
                .Where(g => g.Memberships.Any(m => m.UserId == userId));

            var searchTerm = HttpContext.Request.Query["searchTerm"].FirstOrDefault()?.Trim();

            // Initialize the query with user's groups
            IQueryable<Group> groupsQuery = userGroups;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Fetch groups that match the search term
                var searchedGroups = db.Groups
                    .Where(g => g.Name.Contains(searchTerm) || g.Description.Contains(searchTerm));

                // Combine user's groups with searched groups
                groupsQuery = groupsQuery.Union(searchedGroups);
            }

            var groups = groupsQuery.Distinct().ToList();

            ViewBag.SearchTerm = searchTerm;
            return View(groups);
        }

        [Authorize(Roles = "Admin,User,Moderator")]
        public IActionResult Show(int id)
        {
            var userId = _userManager.GetUserId(User);
            bool isUserInGroup = db.Memberships
                                .Any(m => m.UserId == userId && m.GroupId == id);
            if (isUserInGroup || User.IsInRole("Admin"))
            {

                var data = (from g in db.Groups.Include(g => g.Channels).ThenInclude(c => c.Category)
                            .Include(g => g.Channels).ThenInclude(c => c.Messages)
                            .ThenInclude(m => m.User)
                            where g.Id == id
                            select g).FirstOrDefault();
                return View(data);
            } else
            {
                TempData["message"] = "You need to join the group to see its contents";
                return RedirectToAction("Index", "Home");
            }
        }
        [Authorize(Roles ="User,Admin,Moderator")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
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

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Delete(int id)
        {
            Group? gr = db.Groups.FirstOrDefault(g => g.Id == id);
            if (gr != null)
            {
                db.Groups.Remove(gr);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Group not found.";
                return RedirectToAction("Index");
            }
        }
    }
}
