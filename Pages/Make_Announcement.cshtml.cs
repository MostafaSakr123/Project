using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;

namespace Project.Pages
{
    public class Make_AnnouncementModel : PageModel
    {
        private readonly DB _db;

        [BindProperty]
        [Required(ErrorMessage = "Title is required")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        public string Title { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Description is required")]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; }

        public string Message { get; set; }

        public Make_AnnouncementModel(DB db)
        {
            _db = db;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Get admin username from session
                var adminUsername = HttpContext.Session.GetString("username");
                if (string.IsNullOrEmpty(adminUsername))
                {
                    Message = "Admin session expired. Please login again.";
                    return Page();
                }

                if (_db.CreateAnnouncement(Title, Description, adminUsername))
                {
                    Message = "Announcement created successfully!";
                    ModelState.Clear();
                    return Page();
                }
                Message = "Failed to create announcement";
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
            }

            return Page();
        }
    }
}