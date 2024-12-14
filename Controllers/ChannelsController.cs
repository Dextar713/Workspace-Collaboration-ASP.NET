using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            //Channel channel = db.Channels.Find(id);
            //ViewBag.Channel = channel;
            //return View();

            Channel channel = db.Channels.Include("Messages").Where(c => c.Id == id).First();
            ViewBag.Channel = channel;
            return View();
        }
        // new
        public IActionResult New()
        {
            return View();
            //Channel channel = new Channel();
            //return View(channel);
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

        //[HttpPost]
        //public IActionResult Show([FromForm] Message msg)
        //{
        //    msg.Date = DateTime.Now;
        //    if (ModelState.IsValid)
        //    {
        //        db.Messages.Add(msg);
        //        db.SaveChanges();
        //        return Redirect("/Channels/Show/" + msg.ChannelId);
        //    }
            
        //    Channel c = db.Channels.Include("Messages").Where(c => c.Id == msg.ChannelId).First();
        //    //return Redirect("/Articles/Show/" + comment.ArticleId);
        //    return View(c);
        //}
    }
}
