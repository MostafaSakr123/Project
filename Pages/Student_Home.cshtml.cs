using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace Project.Pages
{
    public class Student_HomeModel : PageModel
    {
        public string StudentName { get; set; }

        public void OnGet()
        {
            // Retrieve student name from the session or default to "Student" if not found
            StudentName = HttpContext.Session.GetString("student_name") ?? "Student";
        }
    }
}