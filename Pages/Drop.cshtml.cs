using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;
using System.Linq;

namespace Project.Pages
{
    public class DropModel : PageModel
    {
        private readonly DB db;

        public DropModel(DB db1)
        {
            db = db1;
        }

        [BindProperty]
        public string CourseCode { get; set; }

        [BindProperty]
        public int Section_Number { get; set; }  // Added this

        public List<RegisteredCourse> RegisteredCourses { get; set; }

        // Called when the page is first loaded (GET request)
        public void OnGet()
        {
            int studentId = HttpContext.Session.GetInt32("studentId") ?? 0;
            RegisteredCourses = db.GetRegisteredCourses(studentId);
        }

        // Called when the "Drop" button is pressed (POST request)
        public IActionResult OnPostDrop()
        {
            int studentId = HttpContext.Session.GetInt32("studentId") ?? 0;

            // Corrected: single predicate with both CourseCode and Section_Number
            var registeredCourse = db.GetRegisteredCourses(studentId)
                                     .FirstOrDefault(c => c.CourseCode == CourseCode && c.SectionNumber == Section_Number);

            if (registeredCourse != null)
            {
                db.DropCourse(studentId, CourseCode, Section_Number);
            }

            return RedirectToPage(); // Refresh the page
        }
    }
}