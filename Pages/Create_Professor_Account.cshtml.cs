using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project.Pages
{
    public class Create_Professor_AccountModel : PageModel
    {
        [BindProperty]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
        public string Name { get; set; }

        [BindProperty]
        public string username { get; set; }
        [MinLength(3, ErrorMessage = "username must be at least 3 characters long.")]

        [BindProperty]
        public string password { get; set; }
        [MinLength(3, ErrorMessage = "password must be at least 3 characters long.")]

        [BindProperty]
        public string Major { get; set; }
        public void OnGet()
        {
        }
    }
}
