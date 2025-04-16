using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project.Pages
{
    public class Add_StudentModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Student name is required")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
        public string Student_Name { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Student ID is required")]
        [Range(100000000, 999999999, ErrorMessage = "ID must be 9 digits")]
        public int Student_ID { get; set; }
        public void OnGet()
        {
        }
    }
}
