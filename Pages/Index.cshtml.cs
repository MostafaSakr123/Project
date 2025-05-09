using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project.Models;

namespace Project.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DB db;

        public string username { get; set; }
        public string role { get; set; }

        public IndexModel(ILogger<IndexModel> logger, DB db1)
        {
            _logger = logger;
            db = db1;
        }

        public IActionResult OnGet()
        {
            // Check if the user is logged in and if the role is Admin
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("username")) || HttpContext.Session.GetString("role") != "Admin")
            {
                return RedirectToPage("/Login");
            }

            // Get user info from session
            username = HttpContext.Session.GetString("username");
            role = HttpContext.Session.GetString("role");

            return Page();
        }

        public IActionResult OnPost()
        {
            // Log out the user
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}
