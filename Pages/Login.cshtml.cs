using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [MinLength(3, ErrorMessage = "username must be at least 3 characters long.")]
        public string username { get; set; }

        [BindProperty]
        [MinLength(3, ErrorMessage = "password must be at least 3 characters long.")]
        public string password { get; set; }
        public void OnGet()
        {
        }
    }
}
