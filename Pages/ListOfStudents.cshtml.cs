using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;

namespace Project.Pages
{
    public class ListOfStudentsModel : PageModel
    {
        private readonly DB _db;
        public DataTable StudentsTable { get; set; }

        public ListOfStudentsModel(DB db)
        {
            _db = db;
        }

        public IActionResult OnGet()
        {
            // Session check and redirect 
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");
            if (string.IsNullOrEmpty(username) || role != "Admin")
            {
                return RedirectToPage("/Login");
            }
          
        StudentsTable = _db.GetTableData("Student");
            return Page();
        }
    }
}