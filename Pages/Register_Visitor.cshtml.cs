using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class Register_as_a_VisitorModel : PageModel
    {
        private readonly DB db;

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string LoginErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public Register_as_a_VisitorModel(DB db1)
        {
            db = db1;
        }


        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                LoginErrorMessage = "Both fields are required.";
                return Page();
            }

            db.SetVisitor(Username, Password);
            SuccessMessage = "Visitor Regisered Successfully.";
            return Page();  // Fallback
        }
        public IActionResult OnPostReturn()
        {
            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password))
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }
    }
}