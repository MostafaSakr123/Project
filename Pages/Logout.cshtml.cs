using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace Project.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            // Remove session data to log out the user
            HttpContext.Session.Remove("role");
            HttpContext.Session.Remove("student_name"); // Optional: Clear any other session data

            // Redirect to the login page
            return RedirectToPage("/Login");
        }
    }
}

