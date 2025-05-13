using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;
using System.Linq;

namespace Project.Pages
{
    public class ViewAnnouncementsModel : PageModel
    {
        private readonly DB _db;
        public List<Announcement> Announcements { get; set; } = new();

        public ViewAnnouncementsModel(DB db) => _db = db;

        public void OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");
            if (int.TryParse(professorId, out int profId))
            {
                Announcements = _db.GetAnnouncements(profId);
            }
        }

        public IActionResult OnPostDelete(int announcementId)
        {
            var professorId = HttpContext.Session.GetString("userId");
            if (int.TryParse(professorId, out int profId))
            {
                var success = _db.DeleteAnnouncement(announcementId, profId);
                TempData["SuccessMessage"] = success ? "Announcement deleted!" : "Error deleting announcement";
            }
            return RedirectToPage();
        }
    }

    public class Announcement
    {
        public int AnnouncementID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}