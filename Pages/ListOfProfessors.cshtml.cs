using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;
using System.Data.SqlClient;

namespace Project.Pages
{
    public class ListOfProfessorsModel : PageModel
    {
        private readonly DB _db;
        public DataTable ProfessorsTable { get; set; } = new DataTable();

        public ListOfProfessorsModel(DB db)
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
         
        ProfessorsTable = _db.GetTableData("Professor");
            return Page();
        }
    }
}