using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly DB _db;

        public Dictionary<string, int> CoursesPerMajor { get; set; } = new();
        public Dictionary<string, Dictionary<string, int>> StudentsPerCourse { get; set; } = new();

        public DashboardModel(DB db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Session check and redirect
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");
            if (string.IsNullOrEmpty(username) || role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            // Load dashboard data
            CoursesPerMajor = await _db.GetCourseCountByMajor();
            StudentsPerCourse = await _db.GetStudentCountByCourse();

            return Page();
        }
    }
}
