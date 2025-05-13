using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Project.Pages
{
    public class Create_CourseModel : PageModel
    {
        private readonly DB _db;

        [BindProperty]
        [Required(ErrorMessage = "Course code is required")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "Code must be 3-10 characters")]
        public string Course_Code { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Course name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be 3-50 characters")]
        public string Course_Name { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Major code is required")]
        public string Major_Code { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Professor ID is required")]
        public int Professor_ID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Number of sections is required")]
        [Range(1, 8, ErrorMessage = "Must have 1-8 sections")]
        public int Number_of_Sections { get; set; }

        public string ErrorMessage { get; set; }

        public Create_CourseModel(DB db)
        {
            _db = db;
        }

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

            // Get admin username from session
            var adminUsername = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(adminUsername))
            {
                ErrorMessage = "Admin session expired. Please login again.";
                return Page();
            }

            // Validate Professor exists
            if (!_db.ProfessorExists(Professor_ID))
            {
                ErrorMessage = $"Professor with ID {Professor_ID} does not exist.";
                return Page();
            }

            // Validate Major exists
            if (!_db.MajorExists(Major_Code))
            {
                ErrorMessage = $"Major code {Major_Code} does not exist.";
                return Page();
            }

            try
            {
                bool success = _db.CreateCourse(
                    Course_Code,
                    Course_Name,
                    Major_Code,
                    adminUsername,
                    Number_of_Sections
                );

                if (!success)
                {
                    ErrorMessage = "Course creation failed. The course code may already exist.";
                    return Page();
                }

                // Add success message
                TempData["SuccessMessage"] = $"Course '{Course_Code} - {Course_Name}' created successfully!";
                return RedirectToPage("/Index");
            }
            catch (SqlException ex)
            {
                ErrorMessage = ex.Number switch
                {
                    2627 => "This course code already exists.",
                    547 => "Invalid reference (major, professor, or admin).",
                    _ => $"Database error: {ex.Message}"
                };
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected error: {ex.Message}";
                return Page();
            }
        }
    }
}