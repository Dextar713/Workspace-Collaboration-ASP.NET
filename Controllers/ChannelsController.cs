using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        // index 
        public IActionResult Index()
        {
            var channels = from channel in db.Channels
                           orderby channel.Id
                           select channel;
            ViewBag.Channels = channels;
            return View();
        }
        // show
        public IActionResult Show(int id)
        {
            Channel channel = db.Channels.Include(c => c.Group)
                .Include(c => c.Messages)
                .ThenInclude(m => m.User).First(c => c.Id == id);
            //ViewBag.Channel = channel;
            return View(channel);
        }
        // new
        
        public IActionResult New(int groupId)
        {
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
        public IActionResult New(Channel c)
        {
            if (ModelState.IsValid)
            {
                //c.GroupId = groupId;
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
        // delete
        public IActionResult Delete(int id)
        {
            Channel? channel = db.Channels.FirstOrDefault(c => c.Id == id);
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
        public IActionResult SendMessage(int channelId, string message)
        {
            var channel = db.Channels.Include(c => c.Group).FirstOrDefault(c => c.Id == channelId);
            System.Diagnostics.Debug.WriteLine($"Message: {message}");
            System.Diagnostics.Debug.WriteLine($"Channel Id: {channelId}");
            if (channel == null || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false });
        }

            // Create a new message
            var userId = _userManager.GetUserId(User);
            var userName = _userManager.GetUserName(User);
            System.Diagnostics.Debug.WriteLine($"UserName: {userName}");
            var newMessage = new Message
            {
                Content = message,
                DateTime = DateTime.Now,
                ChannelId = channelId,
                UserId = userId // Assuming your User model has an ID
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
                    DateTime = newMessage.DateTime.ToString("g")
                }
            };
            
            return Json(responseMessage);
        }
    }
}
