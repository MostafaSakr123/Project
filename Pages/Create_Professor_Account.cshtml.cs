using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Project.Pages
{
    public class Create_Professor_AccountModel : PageModel
    {
        [BindProperty]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
        public string Name { get; set; }

        [BindProperty]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        public string username { get; set; }

        [BindProperty]
        [MinLength(3, ErrorMessage = "Password must be at least 3 characters long.")]
        public string password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Professor ID is required")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "ID must be a 9-digit number.")]
        public string ID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Major Code is required")]
        public string Major { get; set; }

        public string Message { get; set; }

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

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            DB db = new DB();
            try
            {
                if (db.CreateProfessorAccount(username, password, int.Parse(ID), Name, Major))
                {
                    Message = "Professor account created successfully!";
                    ModelState.Clear();
                    return Page();
                }
                Message = "Account creation failed (database returned false)";
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
            }

            return Page();
        }
    }
}

