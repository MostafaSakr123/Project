using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project.Pages
{
    public class Professor_ViewScheduleModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SelectedCourse { get; set; }

        public bool ShouldDisplay(string courseCode)
        {
            return string.IsNullOrEmpty(SelectedCourse) ||
                   SelectedCourse.Equals(courseCode, StringComparison.OrdinalIgnoreCase);
        }

        public void OnGet()
        {
            // Initialization if needed
        }
    }
}