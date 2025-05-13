using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class Professor_AnnouncementsModel : PageModel
    {
        private readonly DB _db;

        [BindProperty, Required]
        public string Title { get; set; }

        [BindProperty, Required]
        public string Description { get; set; }

        public Professor_AnnouncementsModel(DB db)
        {
            _db = db;
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                var professorId = int.Parse(HttpContext.Session.GetString("userId"));
                _db.SaveAnnouncement(professorId, Title, Description, DateTime.Now);

                TempData["SuccessMessage"] = "Posted successfully!";
                return RedirectToPage(); // PRG pattern to prevent duplicate submissions
            }
            return Page();
        }
    }
}