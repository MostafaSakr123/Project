using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Project.Pages
{
    public class Professor_GradesModel : PageModel
    {
        private readonly DB _db;

        [BindProperty]
        [Required(ErrorMessage = "Course is required")]
        public string SelectedCourse { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Student ID is required")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Must be exactly 9 digits")]
        public string StudentId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Course Work score is required")]
        [Range(0, 100, ErrorMessage = "Must be between 0-100")]
        public int AssignmentScore { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Exam score is required")]
        [Range(0, 100, ErrorMessage = "Must be between 0-100")]
        public int ExamScore { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Semester is required")]
        public string semester { get; set; }

        public DataTable ProfessorCourses { get; set; }
        public string CalculatedGrade { get; set; }

        public Professor_GradesModel(DB db)
        {
            _db = db;
            ProfessorCourses = new DataTable();
        }

        public void OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");
            ProfessorCourses = _db.GetProfessorCourses(professorId);
        }

        public IActionResult OnPostCalculate()
        {
            if (!ModelState.IsValid)
            {
                OnGet(); // Reload courses
                return Page();
            }

            CalculateGrade();
            OnGet(); // Reload courses while retaining form values
            return Page();
        }

        public IActionResult OnPostSubmit()
        {
            if (!ModelState.IsValid) return PageWithError();

            if (!int.TryParse(StudentId, out int studentIdInt))
            {
                ModelState.AddModelError("StudentId", "Invalid Student ID format");
                return PageWithError();
            }

            if (!_db.IsStudentEnrolled(studentIdInt, SelectedCourse, semester))
            {
                TempData["ErrorMessage"] = "Student not enrolled in this course";
                return RedirectToPage();
            }

            CalculateGrade();

            if (_db.UpsertGrade(semester, studentIdInt, SelectedCourse, CalculatedGrade))
            {
                TempData["SuccessMessage"] = $"Grade {CalculatedGrade} saved successfully!";
                return RedirectToPage();
            }

            ModelState.AddModelError("", "Failed to save grade");
            return PageWithError();
        }

        private IActionResult PageWithError()
        {
            OnGet();
            return Page();
        }

        private void CalculateGrade()
        {
            double total = (AssignmentScore * 0.6) + (ExamScore * 0.4);
            CalculatedGrade = total switch
            {
                >= 95 => "A",
                >= 90 => "A-",
                >= 85 => "B+",
                >= 80 => "B",
                >= 75 => "B-",
                >= 65 => "C+",
                >= 60 => "C",
                >= 55 => "C-",
                _ => "F"
            };
        }
    }
}