using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project.Pages
{
    public class Update_CourseModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Session check and redirect 
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");
            if (string.IsNullOrEmpty(username) || role != "Admin")
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }
    }
}
