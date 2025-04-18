using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class Professor_GradesModel : PageModel
    {
        [BindProperty, Required(ErrorMessage = "Course is required")]
        public string SelectedCourse { get; set; }

        [BindProperty, Required(ErrorMessage = "Student ID is required")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Must be exactly 9 digits")]
        public string StudentId { get; set; }

        [BindProperty, Required(ErrorMessage = "CourseWork score is required")]
        [Range(0, 100, ErrorMessage = "Must be between 0-100")]
        public int AssignmentScore { get; set; }

        [BindProperty, Required(ErrorMessage = "Exam score is required")]
        [Range(0, 100, ErrorMessage = "Must be between 0-100")]
        public int ExamScore { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool GradeCalculated { get; set; }

        public void OnGet()
        {
            GradeCalculated = false;
        }

        public IActionResult OnPostCalculate()
        {
            GradeCalculated = true;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                GradeCalculated = false;
                return Page();
            }

            TempData["SuccessMessage"] = $"Grade submitted for Student {StudentId}";
            return RedirectToPage();
        }

        public string CalculateGrade()
        {
            var total = (AssignmentScore * 0.6) + (ExamScore * 0.4);
            return total switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }
    }
}