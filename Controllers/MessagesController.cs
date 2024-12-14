using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Mvc;

namespace Discord2.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext db;
        public MessagesController(ApplicationDbContext context)
        {
            db = context;
        }

        // adaugarea unui mesaj asociat unui canal
        [HttpPost]
        public IActionResult New(Message msg)
        {
            msg.Date = DateTime.Now;
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
        public IActionResult Delete(int id)
        {
            Message msg = db.Messages.Find(id);
            db.Messages.Remove(msg);
            db.SaveChanges();
            return Redirect("/Channels/Show/" + msg.ChannelId);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
