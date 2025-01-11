using Discord2.Data;
using Discord2.Hubs;
using Discord2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

namespace Discord2.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHubContext<ChatHub> _hubContext;
        public GroupsController(ApplicationDbContext context, 
                                UserManager<AppUser> userManager,
                                RoleManager<IdentityRole> roleManager,
                                IHubContext<ChatHub> hubcontext) 
        {
            _userManager = userManager;
            _roleManager = roleManager;
            db = context;
            _hubContext = hubcontext;
        }

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            // Fetch groups the user is part of
            var userGroups = db.Groups.Include(g => g.Memberships).ThenInclude(m => m.User)
                .Where(g => g.Memberships!.Any(m => m.UserId == userId));

            var searchTerm = HttpContext.Request.Query["searchTerm"].FirstOrDefault()?.Trim();

            // Initialize the query with user's groups
            IQueryable<Group> groupsQuery = userGroups;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Fetch groups that match the search term
                var searchedGroups = db.Groups.Include(g => g.Memberships).ThenInclude(m => m.User)
                    .Where(g => g.Name.Contains(searchTerm) || g.Description.Contains(searchTerm));

                // Combine user's groups with searched groups
                groupsQuery = groupsQuery.Union(searchedGroups);
            }

            var groups = groupsQuery.Distinct().ToList();
            foreach (var g in groups)
            {
                g.Members = g.Memberships!
                                .Select(m => m.User!)
                                .Distinct()
                                .ToList();
            }
            ViewBag.SearchTerm = searchTerm;
            ViewBag.UserId = userId;    
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
                ViewBag.UserId = userId;
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
                //var connectionId = HttpContext.Connection.Id;
                //_hubContext.Groups.AddToGroupAsync(userId, g.Id.ToString());
                return RedirectToAction("Index");
            } else
            {
                return View();
            }
        }

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Edit(int id)
        {
            var g = db.Groups.FirstOrDefault(g => g.Id == id);
            var userId = _userManager.GetUserId(User);
            var role = (from m in db.Memberships.Include(m => m.Role)
                        where m.UserId == userId
                        && m.GroupId == id
                        select m.Role).FirstOrDefault();
            if(role.Name != "Admin")
            {
                TempData["message"] = "Only admin can edit group";
                return RedirectToAction("Show", "Groups", new { id });
            }
            return View(g);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Edit(int id, Group g)
        {
            var userId = _userManager.GetUserId(User);
            var gr = db.Groups.FirstOrDefault(g => g.Id == id);
            var role = (from m in db.Memberships.Include(m => m.Role)
                        where m.UserId == userId
                        && m.GroupId == id
                        select m.Role).FirstOrDefault();
            if (role.Name != "Admin")
            {
                TempData["message"] = "Only admin can edit group";
                return RedirectToAction("Show", "Groups", new { id });
            }
            if (ModelState.IsValid)
            {
                gr.Name = g.Name;
                gr.Description = g.Description;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(g);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Delete(int id)
        {
            Group? gr = db.Groups.FirstOrDefault(g => g.Id == id);
            var userId = _userManager.GetUserId(User);
            var role = (from m in db.Memberships
                        where m.UserId == userId 
                        && m.GroupId == id 
                        select m.Role).FirstOrDefault();
            if (role.Name!="Admin")
            {
                TempData["message"] = "Only admin can delete group";
                return RedirectToAction("Show", "Groups", new { id });
            }
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

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Members(int id)
        {
            var group = db.Groups.Include(g => g.Memberships).ThenInclude(m => m.User)
                        .FirstOrDefault(g => g.Id == id);
            if(group != null)
            {
                group.Members = group.Memberships
                                .Where(m => m.User != null)
                                .Select(m => m.User!)
                                .Distinct()
                                .ToList();
                group.GroupRoles = db.Memberships
                                   .Where(m => m.Role != null)
                                   .Select(m => m.Role!).Distinct().ToList();
                return View(group);
            } else
            {
                TempData["message"] = "Group not found.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult RemoveMember(int? groupId, string? userId)
        {
            var membership = db.Memberships.FirstOrDefault(m => m.GroupId == groupId 
                                                           && m.UserId == userId);
            var manipulator_id = _userManager.GetUserId(User);
            bool canManipulate = (from m in db.Memberships
                        where m.UserId == manipulator_id
                        && m.GroupId == groupId
                        select m.Role.CanManipulateUsers).First();
            if (!canManipulate && manipulator_id != userId)
            {
                TempData["message"] = "You have no permissions to manipulate users";
                return RedirectToAction("Members", new { id = groupId });
            }
            if (membership != null)
            { 
                db.Memberships.Remove(membership);
                db.SaveChanges();
                if (userId != manipulator_id)
                {
                    TempData["message"] = "User removed from the group!";
                    return RedirectToAction("Members", new { id = groupId });
                } else
                {
                    TempData["message"] = "You left the group!";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["message"] = "Membership of the user in the group not found.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult AddMember(int? groupId, string? userId)
        {
            var roleId = (from role in db.GroupRoles
                          where role.Name == "User"
                          select role.Id).First();
            Membership membership = new Membership
            {
                GroupId = groupId,
                UserId = userId,
                GroupRoleId = roleId
            };
            
            db.Memberships.Add(membership);
            db.SaveChanges();
            //var connectionId = HttpContext.Connection.Id;
            //_hubContext.Groups.AddToGroupAsync(connectionId, groupId.ToString());
            TempData["message"] = "You joined the group!";
            return RedirectToAction("Show", new { id = groupId });
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult AssignRole(int? groupId, string? userId, int? new_role_id)
        {
            if (ModelState.IsValid)
            {
                var manipulator_id = _userManager.GetUserId(User);
                bool canManipulate = (from m in db.Memberships.Include(mem => mem.Role)
                                      where m.GroupId == groupId
                                      && m.UserId == manipulator_id
                                      select m.Role.CanManipulateUsers).First();

                Membership membership = db.Memberships.Include(m => m.Role).Where(m => m.GroupId == groupId
                                                            && m.UserId == userId).FirstOrDefault();
                if (canManipulate)
                {

                    membership.GroupRoleId = new_role_id;
                    db.SaveChanges();
                    return RedirectToAction("Members", new { id = groupId });
                }
                TempData["message"] = "You have no permissions to change user roles";
            } 
            return RedirectToAction("Members", new { id = groupId });
        }
    }
}
