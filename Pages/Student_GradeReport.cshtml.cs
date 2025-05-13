using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;

namespace Project.Pages
{
    public class Student_GradeReportModel : PageModel
    {
        public List<Student_GradeReport> Grades { get; set; } = new List<Student_GradeReport>();

        public IActionResult OnGet()
        {
            // Get student ID from session
            int? studentId = HttpContext.Session.GetInt32("studentId");

            // Redirect to login if studentId not found
            if (!studentId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            // Now it's safe to pass studentId.Value
            DB db = new DB();
            Grades = db.GetStudentGrades(studentId.Value);

            return Page();
        }
    }
}



