using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;

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
        public string Semester { get; set; }

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
                OnGet();
                return Page();
            }

            CalculateGrade();
            OnGet();
            return Page();
        }

        public IActionResult OnPostSubmit()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }

            try
            {
                var professorId = HttpContext.Session.GetString("userId");

                // Validate enrollment for semester
                if (!_db.IsStudentEnrolled(StudentId, SelectedCourse, Semester))
                {
                    ModelState.AddModelError("", "Student is not enrolled in this course for selected semester");
                    OnGet();
                    return Page();
                }

                // Calculate grade
                CalculateGrade();

                // Upsert grade
                if (_db.UpsertGrade(Semester, StudentId, SelectedCourse, CalculatedGrade))
                {
                    TempData["SuccessMessage"] = $"Grade {CalculatedGrade} submitted for {Semester}";
                    return RedirectToPage();
                }

                ModelState.AddModelError("", "Failed to save grade. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            OnGet();
            return Page();
        }

        private void CalculateGrade()
        {
            double total = (AssignmentScore * 0.6) + (ExamScore * 0.4);
            CalculatedGrade = total switch
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