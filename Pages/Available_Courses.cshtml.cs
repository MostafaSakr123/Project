using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Project.Pages
{
    public class Available_CoursesModel : PageModel
    {

        private readonly DB _db;
        public List<Courses> AvailableCourses { get; set; } = new List<Courses>();
        public List<Majors> Major { get; set; } = new List<Majors>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedMajor { get; set; }

        public Available_CoursesModel(DB db)
        {
            _db = db;
        }

        public void OnGet()
        {
            LoadMajors();
            LoadCourses();
        }

        private void LoadMajors()
        {
            string query = "SELECT Major_Code, Major_Name FROM Major";
            DataTable dt = _db.ExecuteReader(query);

            Major = dt.AsEnumerable().Select(row => new Majors
            {
                Code = row["Major_Code"].ToString(),
                Name = row["Major_Name"].ToString()
            }).ToList();
        }

        private void LoadCourses()
        {
            string query = @"
            SELECT c.Course_Code, c.Course_Name, c.Major_Code, m.Major_Name 
            FROM courses c 
            INNER JOIN Major m ON m.Major_Code = c.Major_Code 
            WHERE  m.Major_Code = c.Major_Code ";

            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query += " AND c.Course_Code LIKE @SearchTerm";
                parameters.Add("@SearchTerm", $"%{SearchTerm}%");
            }

            if (!string.IsNullOrEmpty(SelectedMajor))
            {
                query += " AND c.Major_Code = @MajorCode";
                parameters.Add("@MajorCode", SelectedMajor);
            }

            DataTable dt = _db.ExecuteReader(query, parameters);

            AvailableCourses = dt.AsEnumerable().Select(row => new Courses
            {
                Code = row["Course_Code"].ToString(),
                Name = row["Course_Name"].ToString(),
                Major = row["Major_Name"].ToString()
            }).ToList();
        }

        public IActionResult OnPostInfo(string CODE)
        {
            return RedirectToPage("/Course_Details", new { code = CODE });

        }

        public IActionResult OnPostMajor()
        {

            return Page();
        }
        public class Courses
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Major { get; set; }
        }
        public class Majors
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
