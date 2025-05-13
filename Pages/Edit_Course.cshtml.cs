using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;

namespace Project.Pages
{
    public class Edit_CourseModel : PageModel
    {
        private readonly DB _db;
        public List<Course> Courses { get; set; }

        public Edit_CourseModel(DB db)
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
            
            Courses = _db.GetAllCourses();
            return Page();
        }

        public IActionResult OnPostDelete(string courseCode)
        {
            if (_db.DeleteCourse(courseCode))
            {
                TempData["SuccessMessage"] = "Course deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete course";
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteSection(string courseCode, int sectionNumber)
        {
            if (_db.DeleteSection(courseCode, sectionNumber))
            {
                TempData["SuccessMessage"] = "Section deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete section";
            }
            return RedirectToPage();
        }
    }
} 
