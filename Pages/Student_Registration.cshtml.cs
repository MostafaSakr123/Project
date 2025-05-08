using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Project.Models;
using System.Collections.Generic;

namespace Project.Pages
{
    public class Student_RegistrationModel : PageModel
    {
        private const string SessionCartKey = "Cart";
        private readonly DB _db = new DB();

        [BindProperty]
        public string CourseName { get; set; }

        public bool HasSearched { get; set; }
        public string Message { get; set; }

        public List<CourseSection> AvailableSections { get; set; } = new();
        public List<RegisteredCourse> Cart { get; set; } = new();

        public void OnGet()
        {
            LoadCart();
        }

        public void OnPostSearch()
        {
            HasSearched = true;
            LoadCart();
            AvailableSections = _db.GetSectionsByCourseName(CourseName);
        }

        public IActionResult OnPostAddToCart(string courseCode, int sectionNumber,
                                             string day, string startTime, string endTime,
                                             string courseName)
        {
            CourseName = courseName;
            HasSearched = true;

            LoadCart();
            if (!Cart.Exists(c => c.CourseCode == courseCode && c.SectionNumber == sectionNumber))
            {
                Cart.Add(new RegisteredCourse
                {
                    CourseCode = courseCode,
                    SectionNumber = sectionNumber,
                    Day = day,
                    StartTime = startTime,
                    EndTime = endTime
                });
                SaveCart();
            }

            AvailableSections = _db.GetSectionsByCourseName(CourseName);
            return Page();
        }

        public IActionResult OnPostRemoveFromCart(string courseCode, int sectionNumber, string courseName)
        {
            CourseName = courseName;
            HasSearched = true;

            LoadCart();
            Cart.RemoveAll(c => c.CourseCode == courseCode && c.SectionNumber == sectionNumber);
            SaveCart();

            // Don't refresh all results if no course name
            AvailableSections = string.IsNullOrWhiteSpace(CourseName)
                ? new List<CourseSection>()
                : _db.GetSectionsByCourseName(CourseName);

            return Page();
        }

        public IActionResult OnPostRegisterCourses()
        {
            int? sid = HttpContext.Session.GetInt32("studentId");
            if (sid == null) return RedirectToPage("/Login");

            foreach (var item in Cart)
                _db.RegisterCourse(sid.Value, item.CourseCode, item.SectionNumber);

            Cart.Clear();
            SaveCart();
            HasSearched = false;
            CourseName = "";
            Message = "Congrats! Successful Registration.";
            return Page();
        }

        public void OnPost() => OnPostSearch();

        private void LoadCart()
        {
            Cart = new List<RegisteredCourse>();
            var data = HttpContext.Session.GetString(SessionCartKey) ?? "";
            var entries = data.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split('|');
                if (parts.Length == 5)
                {
                    Cart.Add(new RegisteredCourse
                    {
                        CourseCode = parts[0],
                        SectionNumber = int.Parse(parts[1]),
                        Day = parts[2],
                        StartTime = parts[3],
                        EndTime = parts[4]
                    });
                }
            }
        }

        private void SaveCart()
        {
            var entries = new List<string>();
            foreach (var c in Cart)
                entries.Add($"{c.CourseCode}|{c.SectionNumber}|{c.Day}|{c.StartTime}|{c.EndTime}");
            HttpContext.Session.SetString(SessionCartKey, string.Join(";", entries));
        }
    }
}
