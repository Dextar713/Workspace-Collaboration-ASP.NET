using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Mvc;

namespace Discord2.Controllers
{
    public class ChannelsController : Controller
    {
        private readonly ApplicationDbContext db;
        public ChannelsController(ApplicationDbContext context)
        {
            db = context;
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
        public ActionResult Show(int id)
        {
            Channel channel = db.Channels.Find(id);
            ViewBag.Channel = channel;
            return View();
        }
        // new
        public IActionResult New()
        {
            return View();
        }
        [HttpPost]
        public IActionResult New(Channel c)
        {
            try
            {
                db.Channels.Add(c);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View();
            }
        }
        // delete
        public ActionResult Delete(int id)
        {
            Channel channel = db.Channels.Find(id);
            db.Channels.Remove(channel);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
