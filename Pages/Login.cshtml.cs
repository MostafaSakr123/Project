using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Globalization;

namespace Project.Pages
{
    public class LoginModel : PageModel
    {
        private readonly DB db;

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string LoginErrorMessage { get; set; }

        public LoginModel(DB db1)
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

            var userRole = db.GetUserRole(Username, Password);
            if (userRole.Item1 == "Invalid")
            {
                // Invalid credentials, show error message
                LoginErrorMessage = "Invalid username or password.";
                return Page();
            }

            // Store user session information
            HttpContext.Session.SetString("username", Username);
            HttpContext.Session.SetString("role", userRole.Item1);
            HttpContext.Session.SetString("userId", userRole.Item2);



          

            // Get the student name and store in the session if the role is 'Student'
            if (userRole.Item1 == "Student")
            {
                var student = db.GetStudentByUsername(Username); // Fetch the student object
                if (student != null)
                {
                    HttpContext.Session.SetString("student_name", student.Name); // Store student name in session
                    HttpContext.Session.SetInt32("studentId", student.ID);
                    //for the grades part 
                    //I did this for the session to store for example "202000123" (not "student_john").
                    //setin32 (to store 32 bit integers)

                }
            }



            // Redirect user based on their role
            if (userRole.Item1 == "Admin")
            {
                return RedirectToPage("/Index");
            }
            else if (userRole.Item1 == "Professor")
            {
                return RedirectToPage("/Professor_HomePage");
            }
            else if (userRole.Item1 == "Student")
            {
                return RedirectToPage("/Student_Home");
            }
            
            return Page();  // Fallback
        }
        public IActionResult OnPostVisitor()
        {
            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password))
            {
                return RedirectToPage("/Register_Visitor");
            }
            return Page();
        }
    }
}
