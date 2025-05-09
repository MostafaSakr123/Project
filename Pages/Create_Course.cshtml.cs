using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class Create_CourseModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Course name is required")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
        public string Course_Name { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Course code is required")]
        [MinLength(3, ErrorMessage = "Code must be at least 3 characters long.")]
        public string Course_Code { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Major is required")]
        public string Major { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Professor name is required")]
        public string Professor_Name { get; set; }
        public void OnGet()
        {
        }
    }
}
