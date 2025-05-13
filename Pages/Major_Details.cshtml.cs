using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;
using static Project.Pages.Prof_InfoModel;


namespace Project.Pages
{
    public class Major_DetailsModel : PageModel
    {
        private readonly DB _db;

        [BindProperty(SupportsGet = true)]
        public string code { get; set; }
        [BindProperty(SupportsGet = true)]
        public DataTable Details { get; set; } = new DataTable();
        [BindProperty(SupportsGet = true)]
        public MajorDetails CurrentMajor { get; set; } = new MajorDetails();
        public class MajorDetails
        {
            public string Name { get; set; }
            public string MajorCode { get; set; }
            public string Description { get; set; }
        }


        public Major_DetailsModel(DB db)
        {
            _db = db;
        }

        public void OnGet(string code)
        {

            // Get Professor Details
            Details = _db.ExecuteStringPage("SELECT * FROM Major WHERE Major_Code = @CODE", code);
            if (Details.Rows.Count > 0)
            {
                foreach (DataRow row in Details.Rows)
                {
                    CurrentMajor = new MajorDetails
                    {
                        Name = row["Major_Name"].ToString(),
                        MajorCode = row["Major_Code"].ToString(),
                        Description = row["Description"].ToString()

                    };
                }
            }
        }
        public IActionResult OnPostReturn()
        {
            return RedirectToPage("/View_Majors");
        }
    }
}