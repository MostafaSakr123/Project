using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;

namespace Project.Pages
{
    public class Course_DetailsModel : PageModel
    {
        private readonly DB _db;

        [BindProperty(SupportsGet = true)]
        public string code { get; set; }
        [BindProperty(SupportsGet = true)]
        public DataTable Details { get; set; } = new DataTable();
        [BindProperty(SupportsGet = true)]
        public CourseDetails CurrentCourse { get; set; } = new CourseDetails();
        public class CourseDetails
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string MajorCode { get; set; }
            public int NoSections { get; set; }
        }


        public Course_DetailsModel(DB db)
        {
            _db = db;
        }

        public void OnGet(string code)
        {

            // Get Professor Details
            Details = _db.ExecuteStringPage("SELECT * FROM Courses WHERE Course_Code = @CODE", code);
            if (Details.Rows.Count > 0)
            {
                foreach (DataRow row in Details.Rows)
                {
                    CurrentCourse = new CourseDetails
                    {
                        NoSections = Convert.ToInt32(row["Number_of_Sections"]),
                        Name = row["Course_Name"].ToString(),
                        MajorCode = row["Major_Code"].ToString(),
                        Code= row["Course_Code"].ToString()
                    };
                }
            }
        }
        public IActionResult OnPostReturn()
        {
            return RedirectToPage("/Available_Courses");
        }
    }
}