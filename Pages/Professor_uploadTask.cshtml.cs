using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Project.Pages
{
    public class Professor_UploadTaskModel : PageModel
    {
        private readonly DB _db;

        public int DraftCount { get; set; }
        public int TaskCount { get; set; }


        [BindProperty, Required(ErrorMessage = "Course is required")]
        public string SelectedCourse { get; set; }

        public string State { get; set; } = "Not Saved";

        [BindProperty, Required(ErrorMessage = "Task title is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be 5-100 characters")]
        public string TaskTitle { get; set; }

        [BindProperty]
        [StringLength(500, ErrorMessage = "Description too long (max 500 chars)")]
        public string? Description { get; set; }

        [BindProperty, Required(ErrorMessage = "Deadline is required")]
        [FutureDate(ErrorMessage = "Deadline must be in the future")]
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);

        [BindProperty, Required(ErrorMessage = "Total grade is required")]
        [Range(1, 100, ErrorMessage = "Must be between 1-100 points")]
        public int TotalGrade { get; set; } = 100;

        [BindProperty]
        public bool AllowLateSubmission { get; set; }

        public DataTable ProfessorCourses { get; set; }

        public Professor_UploadTaskModel(DB db)
        {
            _db = db;
            ProfessorCourses = new DataTable();
        }

        public void OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");
            ProfessorCourses = _db.GetProfessorCourses(professorId);
            DraftCount = _db.GetDraftCount(int.Parse(professorId));
            TaskCount = _db.GetTaskCount(int.Parse(professorId));

        }

        public IActionResult OnPostSaveDraft()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix validation errors";
                return RedirectToPage();
            }

            try
            {
                _db.SaveTaskDraft(
                    int.Parse(HttpContext.Session.GetString("userId")),
                    SelectedCourse,
                    TaskTitle,
                    Description,
                    Deadline,
                    TotalGrade,
                    AllowLateSubmission
                );
                TempData["SuccessMessage"] = "Draft saved successfully!";
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to save draft";
            }
            return RedirectToPage();
        }

        public IActionResult OnPostPublish()
        {
            if (string.IsNullOrEmpty(SelectedCourse))
            {
                TempData["ErrorMessage"] = "Course selection is invalid";
                return RedirectToPage();
            }
                if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"MODEL ERROR: {error.ErrorMessage}");
                }
                TempData["ErrorMessage"] = "Fix validation errors: " + string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return RedirectToPage();
            }

            try
            {
                _db.PublishTask(
                    int.Parse(HttpContext.Session.GetString("userId")),
                    SelectedCourse,
                    TaskTitle,
                    Description,
                    Deadline,
                    TotalGrade,
                    AllowLateSubmission
                );
                TempData["SuccessMessage"] = "Task published successfully!";
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to publish task: {ex.Message}";
                Console.WriteLine($"Full error: {ex}");

            }
            return RedirectToPage();
        }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value) =>
            value is DateTime date && date > DateTime.Now;
    }
}