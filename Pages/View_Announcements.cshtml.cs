using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;
using System.Data.SqlClient;

namespace Project.Pages
{
    public class View_AnnouncementsModel : PageModel
    {
        private readonly DB _db; // Database class
        [BindProperty(SupportsGet = true)]
        public List<Announcements> Announcements { get; set; } = new List<Announcements>();

        [BindProperty(SupportsGet = true)]
        DataTable dt { get; set; } = new DataTable();

        // Constructor: Dependency Injection
        public View_AnnouncementsModel(DB db)
        {
            _db = db;
        }

        // HTTP GET Handler
        public void OnGet()
        {
            try
            {
                string query = "SELECT * FROM Announcements ORDER BY Time DESC";
                dt = _db.ExecuteReader(query);
                if (dt.Rows.Count == 0)
                {
                    // No error - just no data
                    ModelState.AddModelError("", "No announcements available at this time.");
                }
                else
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        
                        Announcements.Add(new Announcements
                        {
                            ID = Convert.ToInt32(row["ID"]),
                            Time = Convert.ToDateTime(row["Time"]),
                            Admin_Username = row["Admin_Username"].ToString(),
                            Description = row["Description"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the actual error
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred.");
            }
        }
    }

    public class Announcements
    {
        public int ID { get; set; }
        public DateTime Time { get; set; }
        public string Admin_Username { get; set; }
        public string Description { get; set; }
    }
}