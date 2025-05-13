using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;

namespace Project.Pages
{
    public class Uni_ProfsModel : PageModel
    {
        private readonly DB _db;
        public List<Professor> Professors { get; set; } = new List<Professor>();
        public Uni_ProfsModel(DB db)
        {
            _db = db;
        }

        public void OnGet()
        {
            string query = "SELECT ID, Name, Major_Code FROM Professor";
            DataTable dt = _db.ExecuteReader(query);

            foreach (DataRow row in dt.Rows)
            {
                Professors.Add(new Professor
                {
                    ID = Convert.ToInt32(row["ID"]),
                    Name = row["Name"].ToString(),
                    MajorCode = row["Major_Code"].ToString()
                });
            }
        }
        public IActionResult OnPostInfo(int ID)
        {
            return RedirectToPage("/Prof_Info", new { id = ID });

        }

        public class Professor
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string MajorCode { get; set; }
        }
    }
}