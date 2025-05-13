using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Project.Pages
{
    public class ViewAttendanceModel : PageModel
    {
        private readonly DB _db;

        [BindProperty(Name = "course", SupportsGet = true)]
        public string SelectedCourse { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();

        public ViewAttendanceModel(DB db) => _db = db;

        public IActionResult OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");

            // Get all courses taught by the professor if none is selected
            if (string.IsNullOrEmpty(SelectedCourse))
            {
                DataTable allCourses = _db.GetProfessorCourses(professorId);
                List<string> courseList = allCourses.Rows
                    .Cast<DataRow>()
                    .Select(row => row["Course_Code"].ToString())
                    .ToList();

                // Fetch records for all courses
                AttendanceRecords = _db.GetAttendanceRecordsForMultipleCourses(courseList, SelectedDate);
            }
            else
            {
                // Fetch records for the selected course
                AttendanceRecords = _db.GetAttendanceRecords(SelectedCourse, SelectedDate);
            }

            return Page();
        }
    }
}