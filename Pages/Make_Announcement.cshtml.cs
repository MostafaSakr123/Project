using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project.Pages
{
    public class Make_AnnouncementModel : PageModel
    {

        [BindProperty]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")]
        public string Title { get; set; }

        [BindProperty]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters long.")]
        public string Description { get; set; }

        public void OnGet()
        {
        }
    }
}
