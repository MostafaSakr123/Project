using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;

namespace Project.Pages
{
    public class Update_SectionModel : PageModel
    {
        private readonly DB _db;

        [BindProperty(SupportsGet = true)]
        public string CourseCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SectionNumber { get; set; }

        [BindProperty]
        public Section Section { get; set; }

        public List<Student> Students { get; set; }

        public Update_SectionModel(DB db)
        {
            _db = db;
        }

        public IActionResult OnGet()
        {
            // Session check and redirect (paste this at start of every OnGet)
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");
            if (string.IsNullOrEmpty(username) || role != "Admin")
            {
                return RedirectToPage("/Login");
            }
            Section = _db.GetSection(CourseCode, SectionNumber);
            if (Section == null)
            {
                TempData["ErrorMessage"] = "Section not found";
                return RedirectToPage("/Edit_Course");
            }

            Students = _db.GetStudentsBySection(CourseCode, SectionNumber);
            return Page();
        }

        public IActionResult OnPost()
        {
            // Ensure key properties are maintained during post
            Section.CourseCode = CourseCode;
            Section.SectionNumber = SectionNumber;

            // Check for ModelState validity
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "An error occured. Check your inputs.";
                ReloadData();
                return Page();
            }

            // Validate if the ProfessorID and ProfessorName match
            if (!_db.IsProfessorValid(Section.ProfID, Section.ProfessorName))
            {
                ModelState.AddModelError("Section.ProfessorName", "Professor ID and Name do not match.");
                TempData["ErrorMessage"] = "The Professor ID and Name do not match.";
                ReloadData();
                return Page();
            }

            // Proceed with saving the section
            if (_db.UpdateSection(Section))
            {
                TempData["SuccessMessage"] = "Section updated successfully";
                return RedirectToPage("/Edit_Course");
            }

            TempData["ErrorMessage"] = "Failed to update section";
            ReloadData();
            return Page();
        }





        public IActionResult OnPostDeleteStudent(int studentID, string courseCode, int sectionNumber)
        {
            if (_db.DeleteStudentFromSection(studentID, courseCode, sectionNumber))
            {
                TempData["SuccessMessage"] = "Student removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove student.";
            }

            return RedirectToPage(new { courseCode, sectionNumber });
        }

        private void ReloadData()
        {
            Students = _db.GetStudentsBySection(CourseCode, SectionNumber);
        }
    }
}

