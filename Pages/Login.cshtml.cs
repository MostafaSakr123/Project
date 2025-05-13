using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;

namespace Project.Pages
{
    public class LoginModel : PageModel
    {
        private readonly DB db;

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string LoginErrorMessage { get; set; }

        public LoginModel(DB db1)
        {
            db = db1;
        }

        public IActionResult OnGet()
        {
            // Check if user is already logged in
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(role))
            {
                // Redirect based on role
                return role switch
                {
                    "Admin" => RedirectToPage("/Index"),
                    "Professor" => RedirectToPage("/Professor_HomePage"),
                    "Student" => RedirectToPage("/Student_Home"),
                    _ => Page()
                };
            }

            // No active session, show login page
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                LoginErrorMessage = "Both fields are required.";
                return Page();
            }

            var userRole = db.GetUserRole(Username, Password);
            if (userRole.Item1 == "Invalid")
            {
                LoginErrorMessage = "Invalid username or password.";
                return Page();
            }

            // Store user session information
            HttpContext.Session.SetString("username", Username);
            HttpContext.Session.SetString("role", userRole.Item1);
            HttpContext.Session.SetString("userId", userRole.Item2);

            // Redirect based on role
            return userRole.Item1 switch
            {
                "Admin" => RedirectToPage("/Index"),
                "Professor" => RedirectToPage("/Professor_HomePage"),
                "Student" => RedirectToPage("/Student_Home"),
                _ => Page()
            };
        }
    }
}
