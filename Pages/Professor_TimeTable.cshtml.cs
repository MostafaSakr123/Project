using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Data;
using System.Data.SqlClient;

namespace Project.Pages
{
    public class Professor_ViewScheduleModel : PageModel
    {
        private readonly DB _db;

        [BindProperty(SupportsGet = true)]
        public string SelectedCourse { get; set; }

        public DataTable ProfessorSections { get; set; }
        public List<CourseItem> ProfessorCourses { get; set; }

        public Professor_ViewScheduleModel(DB db)
        {
            _db = db;
            ProfessorCourses = new List<CourseItem>();
            ProfessorSections = new DataTable();
        }
      
        public void OnGet()
        {
            var professorId = HttpContext.Session.GetString("userId");
            // Get professor's courses
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = new SqlConnection(_db.con.ConnectionString); // Create new connection
                cmd.CommandText = @"SELECT DISTINCT c.Course_Code, c.Course_Name 
                                  FROM Sections s
                                  JOIN Courses c ON s.Course_Code = c.Course_Code
                                  WHERE s.Prof_ID = @profId";
                cmd.Parameters.AddWithValue("@profId", professorId);

                try
                {
                    cmd.Connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProfessorCourses.Add(new CourseItem
                            {
                                Code = reader["Course_Code"].ToString(),
                                Name = reader["Course_Name"].ToString()
                            });
                        }
                    }
                }
                finally
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                }
            }

            // Get sections
            string query = @"SELECT s.Course_Code, s.Day, s.Start_Time, s.End_Time 
                           FROM Sections s
                           WHERE s.Prof_ID = @profId";

            if (!string.IsNullOrEmpty(SelectedCourse))
            {
                query += " AND s.Course_Code = @courseCode";
            }

            using (var cmd = new SqlCommand(query, new SqlConnection(_db.con.ConnectionString)))
            {
                cmd.Parameters.AddWithValue("@profId", professorId);

                if (!string.IsNullOrEmpty(SelectedCourse))
                {
                    cmd.Parameters.AddWithValue("@courseCode", SelectedCourse);
                }

                try
                {
                    cmd.Connection.Open();
                    ProfessorSections.Load(cmd.ExecuteReader());
                }
                finally
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                }
            }
        }

        public class CourseItem
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}