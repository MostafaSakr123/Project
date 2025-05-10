using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;
using System.Linq;

namespace Project.Pages
{
    public class ViewGradesModel : PageModel
    {
        private readonly DB _db;
        public Dictionary<string, List<StudentGrade>> GroupedGrades { get; set; } = new();

        public ViewGradesModel(DB db) => _db = db;

        public void OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");
            if (int.TryParse(professorId, out int profId))
            {
                // Fetch grades for courses taught by this professor
                var grades = _db.GetGradesByProfessor(profId);

                // Group by Course Code and sort grades within each group
                GroupedGrades = grades
                    .GroupBy(g => g.CourseCode)
                    .OrderBy(g => g.Key) // Sort courses alphabetically
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(grade => grade.LetterGradeOrder).ToList()
                    );
            }
        }
    }
}