using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class Professor_AnnouncementsModel : PageModel
    {
        [BindProperty, Required]
        public string Title { get; set; }

        [BindProperty, Required]
        public string Description { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                // Save logic here
                ModelState.Clear(); // Clear the form
                TempData["SuccessMessage"] = "Posted successfully!";
            }
            return Page();
        }
    }
}