using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Discord2.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public MessagesController(ApplicationDbContext context,
                                  UserManager<AppUser> userManager,
                                  RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // adaugarea unui mesaj asociat unui canal
        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult New(Message msg)
        {
            msg.DateTime = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Messages.Add(msg);
                db.SaveChanges();
                //return Redirect("/Channels/Show/" + msg.ChannelId);
            }
            return Redirect("/Channels/Show/" + msg.ChannelId);
        }

        // stergerea unui mesaj asociat unui canal
        [HttpPost]
        [Authorize(Roles = "User,Admin,Moderator")]
        public IActionResult Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var role = (from m in db.Memberships
                        where m.UserId == userId
                        select m.Role).FirstOrDefault();
            Message msg = db.Messages.First(m => m.Id == id);
            if (msg.UserId == userId || role.CanManipulateUsers)
            {
                db.Messages.Remove(msg);
                db.SaveChanges();
                return Redirect("/Channels/Show/" + msg.ChannelId);
            }
            TempData["message"] = "You cannot delete messages of other users";
            return Redirect("/Channels/Show/" + msg.ChannelId);
        }
    }
}
