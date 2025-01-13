using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
// System.Threading.Channels;

namespace Discord2.Controllers
{
    public class ChannelsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ChannelsController(ApplicationDbContext context,
                                  UserManager<AppUser> userManager,
                                  RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Index()
        {
            var channels = from channel in db.Channels
                           orderby channel.Id
                           select channel;
            ViewBag.Channels = channels;
            return View();
        }

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Show(int id)
        {
            Channel channel = db.Channels.Include(c => c.Group)
                .Include(c => c.Messages)
                .ThenInclude(m => m.User).First(c => c.Id == id);
            var userId = _userManager.GetUserId(User);
            //ViewBag.UserId = userId;
            SetCurRole(channel.GroupId);
            return View(channel);
        }

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult New(int groupId)
        {
            var userId = _userManager.GetUserId(User);
            var role = (from m in db.Memberships
                        where m.UserId == userId
                        && m.GroupId == groupId
                        select m.Role).FirstOrDefault();
            if (!role.HasSecretChannelsAccess)
            {
                TempData["message"] = "You have no permissions to create channels";
                return RedirectToAction("Show", "Groups", new { id = groupId });
            }
            var group = db.Groups.FirstOrDefault(g => g.Id == groupId);
            if (group == null)
            {
                TempData["message"] = "Group not found.";
                return RedirectToAction("Show", "Groups", new { id = groupId });
            }

            var model = new Channel
            {
                GroupId = groupId
            };
            ViewBag.Categories = db.Categories.Select(c => new { c.Id, c.Name }).ToList();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult New(Channel c)
        {
            var userId = _userManager.GetUserId(User);
            var role = (from m in db.Memberships
                        where m.UserId == userId
                        && m.GroupId == c.GroupId
                        select m.Role).FirstOrDefault();
            if (!role.HasSecretChannelsAccess)
            {
                TempData["message"] = "You have no permissions to create channels";
                return RedirectToAction("Show", "Groups", new { id = c.GroupId });
            }
            if (ModelState.IsValid)
            {
                db.Channels.Add(c);
                db.SaveChanges();
                TempData["message"] = "Channel created successfully!";
                return RedirectToAction("Show", "Groups", new { id = c.GroupId });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["message"] = string.Join("; ", errors);
            ViewBag.Categories = db.Categories.Select(c => new { c.Id, c.Name }).ToList();
            return View(c);
        }

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Edit(int id)
        {
            var channel = db.Channels.FirstOrDefault(c => c.Id == id);
            var userId = _userManager.GetUserId(User);
            var role = (from m in db.Memberships
                        where m.UserId == userId
                        && m.GroupId == channel.GroupId
                        select m.Role).FirstOrDefault();
            if (!role.HasSecretChannelsAccess && !User.IsInRole("Admin"))
            {
                TempData["message"] = "You have no permissions to edit channels";
                return RedirectToAction("Show", "Groups", new { id = channel.GroupId });
            }
            ViewBag.Categories = db.Categories.Select(c => new { c.Id, c.Name }).ToList();
            return View(channel);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Edit(int id, Channel c)
        {
            var userId = _userManager.GetUserId(User);
            var role = (from m in db.Memberships
                        where m.UserId == userId
                        && m.GroupId == c.GroupId
                        select m.Role).FirstOrDefault();
            if (!role.HasSecretChannelsAccess && !User.IsInRole("Admin"))
            {
                TempData["message"] = "You have no permissions to edit channels";
                return RedirectToAction("Show", "Groups", new { id = c.GroupId });
            }
            var channel = db.Channels.FirstOrDefault(c => c.Id == id);
            if (ModelState.IsValid)
            {
                channel.Name = c.Name;
                channel.Category = c.Category;
                db.SaveChanges();
                TempData["message"] = "Channel edited successfully!";
                return RedirectToAction("Show", "Groups", new { id = c.GroupId });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["message"] = string.Join("; ", errors);
            ViewBag.Categories = db.Categories.Select(c => new { c.Id, c.Name }).ToList();
            return View(c);
        }

        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Delete(int id)
        {
            Channel? channel = db.Channels.FirstOrDefault(c => c.Id == id);
            var userId = _userManager.GetUserId(User);
            var role = (from m in db.Memberships
                        where m.UserId == userId
                        && m.GroupId == channel.GroupId
                        select m.Role).FirstOrDefault();
            if (!role.HasSecretChannelsAccess && !User.IsInRole("Admin"))
            {
                TempData["message"] = "You have no permissions to delete channels";
                return RedirectToAction("Show", "Groups", new { id = channel.GroupId });
            }
            if (channel != null)
            {
                db.Channels.Remove(channel);
                db.SaveChanges();
                return RedirectToAction("Show", "Groups", new { id = channel.GroupId });
            } else
            {
                TempData["message"] = "Channel not found.";
                return RedirectToAction("Index", "Groups");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult SendMessage(int channelId, string message)
        {
            var channel = db.Channels.Include(c => c.Group).FirstOrDefault(c => c.Id == channelId);
            //System.Diagnostics.Debug.WriteLine($"Message: {message}");
            //System.Diagnostics.Debug.WriteLine($"Channel Id: {channelId}");
            if (channel == null || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false });
        }

            // Create a new message
            var userId = _userManager.GetUserId(User);
            bool canWrite = db.Memberships.Include(m => m.Role).Where(m => m.GroupId == channel.GroupId
                                                && m.UserId == userId)
                                           .Select(m => m.Role.CanWrite).First();
            if(!canWrite)
            {
                return Json(new
                {
                    success = false,
                    error_msg = "You are banned and can't write in channels"
                });
            }
            var userName = _userManager.GetUserName(User);
            //System.Diagnostics.Debug.WriteLine($"UserName: {userName}");
            var newMessage = new Message
            {
                Content = message,
                DateTime = DateTime.Now,
                ChannelId = channelId,
                UserId = userId 
            };

            // Add the message to the database
            db.Messages.Add(newMessage);
            db.SaveChanges();

            // Return the new message as a JSON response
            var responseMessage = new
            {
                success = true,
                message = new
                {
                    Id = newMessage.Id,
                    UserName = userName,
                    Content = newMessage.Content,
                    DateTime = newMessage.DateTime.ToString("g"),
                    GroupId = channel.GroupId,
                    ChannelId = channelId
                }
            };
            
            return Json(responseMessage);
        }

        [NonAction]
        private void SetCurRole(int? groupId)
        {
            var userId = _userManager.GetUserId(User);
            GroupRole role = db.Memberships
                         .Where(m => m.UserId == userId
                                && m.GroupId == groupId)
                         .Select(m => m.Role).First();
            ViewBag.Role = role;
            ViewBag.UserId = userId;
        }
    }
}
