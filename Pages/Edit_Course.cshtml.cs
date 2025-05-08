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

        public void OnGet()
        {
            Courses = _db.GetAllCourses();
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
