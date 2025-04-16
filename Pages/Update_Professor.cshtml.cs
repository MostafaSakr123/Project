using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project.Pages
{
    public class Update_ProfessorModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Professor name is required")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
        public string Professor_Name { get; set; }
        public void OnGet()
        {
        }
    }
}
