using Discord2.Data;
using Discord2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Discord2.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CategoriesController(ApplicationDbContext context,
                                    UserManager<AppUser> userManager,
                                    RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize(Roles ="User,Admin,Moderator")]
        public ActionResult Index()
        {
            var categories = from category in db.Categories
                             orderby category.Name
                             select category;
            return View(categories);
        }
        [Authorize(Roles = "Admin,Moderator")]
        public ActionResult Show(int id)
        {
            Category category = db.Categories.First(cat => cat.Id == id);
            return View(category);
        }
        [Authorize(Roles = "Admin,Moderator")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public ActionResult New(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                TempData["message"] = "Category added successfully!";
                return RedirectToAction("Index");
            }

            return View(cat);
        }
       

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.First(cat => cat.Id == id);
            if (ModelState.IsValid)
            { 
                {
                    category.Name = requestCategory.Name;
                    db.SaveChanges();
                }
                TempData["message"] = "Category edited successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                return View("Show", category);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.First(cat => cat.Id == id);
            if (category == null)
            {
                TempData["message"] = "Category not found.";
                return RedirectToAction("Index");
            }
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
