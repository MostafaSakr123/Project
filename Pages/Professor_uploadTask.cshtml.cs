using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class Professor_UploadTaskModel : PageModel
    {
        [BindProperty, Required(ErrorMessage = "Course selection is required")]
        public string SelectedCourse { get; set; }

        [BindProperty]
        public string State { get; set; } = "Not Saved";

        [BindProperty, Required(ErrorMessage = "Task title is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be 5-100 characters")]
        public string TaskTitle { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty, Required(ErrorMessage = "Deadline is required")]
        [FutureDate(ErrorMessage = "Deadline must be in the future")]
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);

        [BindProperty, Required(ErrorMessage = "Total grade is required")]
        [Range(1, 1000, ErrorMessage = "Must be between 1-1000 points")]
        public int TotalGrade { get; set; } = 100;

        [BindProperty]
        public bool AllowLateSubmission { get; set; }

        public void OnGet()
        {
            // Initialize default values if needed
        }

        public IActionResult OnPostSaveDraft()
        {
            State = "Draft Saved";
            return Page(); // Stay on same page with form data preserved
        }

        public IActionResult OnPostPublish()
        {
            if (!ModelState.IsValid)
                return Page();

            State = "Published";
            TempData["SuccessMessage"] = "Task published successfully!";
            return RedirectToPage(); // Clear form
        }

        public class FutureDateAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                return value is DateTime date && date > DateTime.Now;
            }
        }
    }
}