using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;

namespace Project.Pages
{
    public class Student_TimetableModel : PageModel
    {
        private readonly DB db;


        public Student_TimetableModel(DB db1)
        {
            db = db1;
        }
        public List<Student_Timetable> Timetable { get; set; }

        public IActionResult OnGet()
        {
            int? studentId = HttpContext.Session.GetInt32("studentId");

            if (studentId == null)
            {
                return RedirectToPage("/Login"); // Redirect if not logged in
            }

            DB db = new DB();
            Timetable = db.GetStudentTimetable(studentId.Value);

            return Page();
        }
    }
}

