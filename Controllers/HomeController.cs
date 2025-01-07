using System.Diagnostics;
using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discord2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,
                              UserManager<AppUser> userManager,
                              RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var groups = db.Groups.Include(g => g.Memberships)
                         .ThenInclude(m => m.User).Take(5).ToList();
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;
            foreach (var g in groups)
            {
                g.Members = g.Memberships!
                                .Select(m => m.User!)
                                .Distinct()
                                .ToList();
            }
            ViewBag.TopGroups = groups;
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
