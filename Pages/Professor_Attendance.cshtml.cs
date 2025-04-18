using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Pages
{
    public class Professor_AttendanceModel : PageModel  
    {
        [BindProperty, Required(ErrorMessage = "Course selection is required")]
        public string SelectedCourse { get; set; }

        [BindProperty, Required(ErrorMessage = "Date is required")]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        [BindProperty]
        public List<AttendanceRecord> Attendances { get; set; } = new()
        {
            new AttendanceRecord(),
            new AttendanceRecord(),
            new AttendanceRecord() // Default 3 rows
        };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            TempData["SuccessMessage"] = $"Attendance recorded for {SelectedCourse}";
            return RedirectToPage();
        }

        public class AttendanceRecord
        {
            [Required(ErrorMessage = "Student ID required")]
            [StringLength(9, MinimumLength = 9, ErrorMessage = "ID must be 9 characters")]
            public string StudentId { get; set; }

            public bool IsPresent { get; set; } = true;
        }
    }
}