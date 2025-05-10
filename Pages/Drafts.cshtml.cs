using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Project.Models;


namespace Project.Pages // Correct namespace
{
    public class DraftsModel : PageModel
    {
        private readonly DB _db;

        public List<Draft> Drafts { get; set; } = new();

        public DraftsModel(DB db) => _db = db;

        public void OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");
            Drafts = _db.GetDrafts(int.Parse(professorId));
        }

        public IActionResult OnPostPublish(int taskId)
        {
            try
            {
                _db.PublishTask(taskId);
                TempData["SuccessMessage"] = "Task published successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Publish failed: {ex.Message}";
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int taskId)
        {
            try
            {
                _db.DeleteDraft(taskId);
                TempData["SuccessMessage"] = "Draft deleted!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Delete failed: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}
