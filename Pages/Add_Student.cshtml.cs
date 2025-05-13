using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class Add_StudentModel : PageModel
    {
        private readonly DB _db;

        public Add_StudentModel(DB db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string CourseCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SectionNumber { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Student ID is required")]
        [Range(100000000, 999999999, ErrorMessage = "ID must be 9 digits")]
        public int Student_ID { get; set; }

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
                return Page();

            if (!_db.DoesStudentExist(Student_ID))
            {
                ModelState.AddModelError(string.Empty, "Student does not exist.");
                return Page();
            }

            // Check if student is already enrolled
            if (_db.IsStudentEnrolled(Student_ID, CourseCode, SectionNumber))
            {
                ModelState.AddModelError(string.Empty,
                    $"Student {Student_ID} is already enrolled in this course section.");
                return Page();
            }

            bool success = _db.EnrollStudent(Student_ID, CourseCode, SectionNumber);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Failed to enroll student.");
                return Page();
            }

            TempData["SuccessMessage"] = "Student successfully enrolled.";
            return RedirectToPage("/Update_Section", new { courseCode = CourseCode, sectionNumber = SectionNumber });
        }
    }
}
