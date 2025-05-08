using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;

namespace Project.Pages
{
    public class IndexModel : PageModel
    {
        public string DisplayName { get; set; }

        public IActionResult OnGet()
        {
            // Check if admin is logged in
            if (HttpContext.Session.GetString("role") != "Admin")
            {
                return RedirectToPage("/Login");
            }

            // Get username from session
            var username = HttpContext.Session.GetString("username") ?? "Admin";

            // Extract name after underscore or use full username
            DisplayName = username.Contains('_')
                ? username.Split('_')[1]
                : username;

            // Capitalize first letter
            if (DisplayName.Length > 0)
            {
                DisplayName = char.ToUpper(DisplayName[0]) + DisplayName.Substring(1).ToLower();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}