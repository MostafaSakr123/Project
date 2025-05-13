using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;

namespace Project.Pages
{
    public class View_MajorsModel : PageModel
    {
        private readonly DB _db;
        public List<Majors> Major { get; set; } = new List<Majors>();

        public View_MajorsModel(DB db)
        {
            _db = db;
        }

        public void OnGet()
        {
            string query = "SELECT * FROM Major";
            DataTable dt = _db.ExecuteReader(query);


            foreach (DataRow row in dt.Rows)
            {
                Major.Add(new Majors
                {
                    Name = row["Major_Name"].ToString(),
                    MajorCode = row["Major_Code"].ToString()
                });
            }
        }
        public IActionResult OnPostInfo(string CODE)
        {
            return RedirectToPage("/Major_Details", new { code = CODE });

        }

        public class Majors
        {
            public string Name { get; set; }
            public string MajorCode { get; set; }
        }
    }
}