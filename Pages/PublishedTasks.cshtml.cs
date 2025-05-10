using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;

namespace Project.Pages
{
    public class PublishedTasksModel : PageModel
    {
        private readonly DB _db;
        public List<PublishedTask> PublishedTasks { get; set; } = new();

        public PublishedTasksModel(DB db) => _db = db;

        public void OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");
            PublishedTasks = _db.GetPublishedTasks(int.Parse(professorId));
        }

        public IActionResult OnPostDelete(int taskId)
        {
            try
            {
                _db.DeleteTask(taskId);
                TempData["SuccessMessage"] = "Task deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Delete failed: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}