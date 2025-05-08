using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Project.Pages
{
    public class Create_Student_AccountModel : PageModel
    {
        private readonly DB _db;

        [BindProperty]
        [Required(ErrorMessage = "Name is required")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        public string Name { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(3, ErrorMessage = "Password must be at least 3 characters")]
        public string Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Student ID is required")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "ID must be a 9-digit number")]
        public string StudentId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Major Code is required")]
        public string MajorCode { get; set; }

        public string Message { get; set; }

        public Create_Student_AccountModel(DB db)
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
                if (_db.CreateStudentAccount(Username, Password, int.Parse(StudentId), Name, MajorCode))
                {
                    Message = "Student account created successfully!";
                    ModelState.Clear();
                    return Page();
                }
                Message = "Account creation failed (database returned false)";
            }
            catch (SqlException ex) when (ex.Number == 2627) // Primary key violation
            {
                Message = "Student ID or username already exists";
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
            }

            return Page();
        }
    }
}