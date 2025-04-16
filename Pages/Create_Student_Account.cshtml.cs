using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project.Pages
{
    public class Create_Student_AccountModel : PageModel
    {


        [BindProperty]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
        public string Name { get; set; }

        [BindProperty]
        [Range(100000000, 999999999, ErrorMessage = "ID must be 9 digits")]
        public int ID { get; set; }


        [BindProperty]
        [MinLength(3, ErrorMessage = "username must be at least 3 characters long.")]
        public string username { get; set; }
        

        [BindProperty]
        [MinLength(3, ErrorMessage = "password must be at least 3 characters long.")]
        public string password { get; set; }

        [BindProperty]
        public string Major { get; set; }
        public void OnGet()
        {
        }
    }
}
