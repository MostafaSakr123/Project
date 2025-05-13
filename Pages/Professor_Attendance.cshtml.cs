using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Project.Pages
{
    public class Professor_AttendanceModel : PageModel
    {
        private readonly DB _db;

        [BindProperty]
        [Required(ErrorMessage = "Course is required")]
        public string SelectedCourse { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Date is required")]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        [BindProperty]
        public List<AttendanceRecord> Attendances { get; set; } = new()
        {
            new AttendanceRecord(),
            new AttendanceRecord(),
            new AttendanceRecord()
        };

        public DataTable ProfessorCourses { get; set; }

        public Professor_AttendanceModel(DB db)
        {
            _db = db;
            ProfessorCourses = new DataTable();
        }

        public void OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");
            ProfessorCourses = _db.GetProfessorCourses(professorId);
        }

        public IActionResult OnPostSaveAttendance()
        {
            var validRecords = Attendances.Where(r => !string.IsNullOrEmpty(r.StudentId)).ToList();
            if (validRecords.Count == 0)
            {
                TempData["ErrorMessage"] = "At least one student ID is required.";
                return RedirectToPage();
            }
            else if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix validation errors";
                return RedirectToPage();
            }
            

       
            try
            {
                var professorId = int.Parse(HttpContext.Session.GetString("userId"));

                foreach (var record in validRecords)
                {
                    if (!_db.IsStudentEnrolled(int.Parse(record.StudentId), SelectedCourse))
                    {
                        TempData["ErrorMessage"] = $"Student {record.StudentId} not enrolled";
                        return RedirectToPage();
                    }

                    _db.SaveAttendance(
                        professorId,
                        SelectedCourse,
                        int.Parse(record.StudentId),
                        AttendanceDate,
                        record.IsPresent ? "Present" : "Absent"
                    );
                }

                TempData["SuccessMessage"] = "Attendance saved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToPage();
        }

        public class AttendanceRecord
        {
            [StringLength(9, MinimumLength = 9, ErrorMessage = "Must be 9 digits")]
            public string StudentId { get; set; }

            public bool IsPresent { get; set; } = true;
        }
    }
}