using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using static Project.Pages.Uni_ProfsModel;

namespace Project.Pages
{
    public class Prof_InfoModel : PageModel
    {
        private readonly DB _db;

        [BindProperty(SupportsGet = true)]
        public int id { get; set; }
        [BindProperty(SupportsGet = true)]
        public DataTable Details { get; set; } = new DataTable();
        [BindProperty(SupportsGet = true)]
        public DataTable AssignedCourses { get; set; } = new DataTable();
        [BindProperty(SupportsGet = true)]
        public ProfessorDetails CurrentProfessor { get; set; } = new ProfessorDetails();
        public class ProfessorDetails
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string MajorCode { get; set; }
        }


        public Prof_InfoModel(DB db)
        {
            _db = db;
        }

        public void OnGet()
        {

            // Get Professor Details
            Details = _db.ExecutePage(@"SELECT * FROM Professor WHERE ID = @ID;", id);


            if (Details.Rows.Count > 0)
            {
                foreach (DataRow row in Details.Rows)
                {
                    CurrentProfessor = new ProfessorDetails
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        Name = row["Name"].ToString(),
                        MajorCode = row["Major_Code"].ToString()
                    };
                }

            }

            // Get Assigned Courses
            AssignedCourses = _db.ExecutePage(
            @"SELECT c.Course_Code, c.Course_Name 
          FROM Courses c 
          JOIN Sections s ON c.Course_Code = s.Course_code 
          WHERE s.Prof_ID = @ID 
          ORDER BY c.Course_Code;", id);

        }
        public IActionResult OnPostReturn()
        {
            return RedirectToPage("/Uni_Profs");
        }
    }
}