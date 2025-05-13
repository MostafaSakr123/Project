using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using Project.Models;

namespace Project.Pages
{
    public class Student_RegistrationModel : PageModel
    {
        private const string SessionKey = "SelectedSection";
        private readonly DB _db = new DB();

        [BindProperty(SupportsGet = true)]
        public string CourseName { get; set; }

        public bool HasSearched { get; set; }
        public string Message { get; set; }

        public List<CourseSection> AvailableSections { get; set; } = new();
        public RegisteredCourse Selected { get; set; }

        public void OnGet()
        {
            LoadSelected();

            if (!string.IsNullOrWhiteSpace(CourseName))
            {
                HasSearched = true;
                CourseName = CourseName.Trim();

                int? sid = HttpContext.Session.GetInt32("studentId");
                if (sid.HasValue)
                {
                    var enrolled = _db.GetRegisteredCourses(sid.Value)
                                      .Select(rc => rc.CourseCode)
                                      .ToHashSet();
                    if (enrolled.Contains(CourseName))
                    {
                        Message = $"You have already registered for {CourseName}.";
                        return;
                    }
                }

                AvailableSections = _db.GetSectionsByCourseName(CourseName);
            }
        }

        public IActionResult OnPostAddToCart(string courseCode, int sectionNumber)
        {
            // keep search state
            HasSearched = true;
            CourseName = CourseName?.Trim();
            LoadSelected();
            AvailableSections = _db.GetSectionsByCourseName(CourseName);

            var sec = AvailableSections
                      .FirstOrDefault(s =>
                          s.CourseCode == courseCode &&
                          s.SectionNumber == sectionNumber);

            if (sec != null)
            {
                Selected = new RegisteredCourse
                {
                    CourseCode = sec.CourseCode,
                    SectionNumber = sec.SectionNumber,
                    Day = sec.Day,
                    StartTime = sec.StartTime,
                    EndTime = sec.EndTime
                };
                SaveSelected();
            }

            return Page();
        }

        public IActionResult OnPostRemoveFromCart()
        {
            Selected = null;
            HttpContext.Session.Remove(SessionKey);
            return Page();
        }

        public IActionResult OnPostRegister()
        {
            LoadSelected();
            int? studentId = HttpContext.Session.GetInt32("studentId");
            if (studentId == null)
            { return RedirectToPage("/Login"); }
                

            if (Selected != null)
            {
                bool ok = _db.RegisterCourse(
                    studentId.Value,
                    Selected.CourseCode,
                    Selected.SectionNumber);

                if (ok)
                    Message = $"Successfully registered for {Selected.CourseCode}.";
                else
                    Message = "Registration failed.";

                Selected = null;
                HttpContext.Session.Remove(SessionKey);
            }

            // re-run search to update sections/duplicate check
            return RedirectToPage(new { CourseName });
        }

        private void LoadSelected()
        {
            var raw = HttpContext.Session.GetString(SessionKey);
            if (!string.IsNullOrEmpty(raw))
            {
                var p = raw.Split('|');
                if (p.Length == 5 && int.TryParse(p[1], out int secNo))
                {
                    Selected = new RegisteredCourse
                    {
                        CourseCode = p[0],
                        SectionNumber = secNo,
                        Day = p[2],
                        StartTime = p[3],
                        EndTime = p[4]
                    };
                }
            }
        }

        private void SaveSelected()
        {
            var raw = $"{Selected.CourseCode}|{Selected.SectionNumber}|"
                    + $"{Selected.Day}|{Selected.StartTime}|{Selected.EndTime}";
            HttpContext.Session.SetString(SessionKey, raw);
        }
    }
}