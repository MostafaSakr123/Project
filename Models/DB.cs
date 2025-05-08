using System.Data.SqlClient;
using System.Data;

namespace Project.Models
{
    public class DB
    {

        public SqlConnection con { get; set; }
        public DB()
        {
            string conStr = "Data Source =LAPTOP-L5C16VL4;Initial Catalog=University; Integrated Security=True;TrustServerCertificate=True";
            con = new SqlConnection(conStr);
        }

        // This fn returns a Tuple<string, string> → (role, id):
        public Tuple<string, string> GetUserRole(string username, string password)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;

                if (con.State != ConnectionState.Open)
                    con.Open();

                // Check Admin
                cmd.CommandText = "SELECT Username FROM Admin WHERE Username = @username AND Password = @password";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                object adminResult = cmd.ExecuteScalar();
                if (adminResult != null)
                    return Tuple.Create("Admin", adminResult.ToString());

                // Check Professor
                cmd.CommandText = @"SELECT P.UserID 
                            FROM Professor P 
                            JOIN [User] U ON P.UserID = U.UserID 
                            WHERE U.Username = @username AND U.Password = @password";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                object profResult = cmd.ExecuteScalar();
                if (profResult != null)
                    return Tuple.Create("Professor", profResult.ToString());

                // Check Student
                cmd.CommandText = @"SELECT S.UserID 
                            FROM Student S 
                            JOIN [User] U ON S.UserID = U.UserID 
                            WHERE U.Username = @username AND U.Password = @password";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                object studentResult = cmd.ExecuteScalar();
                if (studentResult != null)
                    return Tuple.Create("Student", studentResult.ToString());
            }

            return Tuple.Create("Invalid", "");
        }

        public string GetAdminName(int id)
        {
            string name = "";

            using (SqlCommand cmd = new SqlCommand("SELECT Name FROM Admin WHERE ID = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);

                if (con.State != ConnectionState.Open)
                    con.Open();

                object result = cmd.ExecuteScalar();
                if (result != null)
                    name = result.ToString();

                con.Close();
            }

            return name;
        }

        public List<Student_GradeReport> GetStudentGrades(int studentId)
        {
            List<Student_GradeReport> grades = new List<Student_GradeReport>();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;
               
                cmd.CommandText = @"SELECT C.Course_Name, G.letter_grade
                            FROM Grades G
                            JOIN Courses C ON G.course_code = C.Course_Code
                            WHERE G.student_id = @studentId";  // I did this to make sure about int datatype & ID not username
               
                cmd.Parameters.AddWithValue("@studentId", studentId);

              

                if (con.State != ConnectionState.Open)
                    con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        grades.Add(new Student_GradeReport
                        {
                            CourseName = reader["Course_Name"].ToString(),
                            Grade = reader["letter_grade"].ToString()  // same column names like in the sql query
                        });
                    }
                }

                con.Close();
            }

            return grades;

        }
        public Student GetStudentByUsername(string username)
        {
           
            using (var command = new SqlCommand(
    "SELECT s.ID, s.Name, u.Username " +
    "FROM Student s JOIN [User] u ON s.UserID = u.UserID " +
    "WHERE u.Username = @Username", con))

            {
                
                command.Parameters.AddWithValue("@Username", username);

                if (con.State != ConnectionState.Open)
                    con.Open();  // Ensuring that the connection is open

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var student = new Student
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Username = reader.GetString(reader.GetOrdinal("Username"))
                    };

                    return student;
                }

                return null;
            }
        }
        public List<Student_Timetable> GetStudentTimetable(int studentId)
        {
            var timetable = new List<Student_Timetable>();
            if (con.State != ConnectionState.Open)
                con.Open();
            {
                string query = @"<PASTE THE ABOVE SQL QUERY HERE>";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    timetable.Add(new Student_Timetable
                    {
                        Day = reader["Day"].ToString(),
                        StartTime = reader["StartTime"].ToString(),
                        EndTime = reader["EndTime"].ToString(),
                        CourseCode = reader["Course_Code"].ToString(),
                        CourseName = reader["Course_Name"].ToString(),
                        SectionNumber = Convert.ToInt32(reader["Section_Number"])
                    });
                }
            }
            return timetable;
        }
        public List<CourseSection> GetSectionsByCourseName(string searchTerm)
        {
            var sections = new List<CourseSection>();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = @"
            SELECT S.Section_Number, S.Course_Code, S.Day, S.Start_Time, S.End_Time
            FROM Sections S
            JOIN Courses C ON S.Course_Code = C.Course_Code
            WHERE C.Course_Name  LIKE @term
               OR C.Course_Code  LIKE @term
        ";
                cmd.Parameters.AddWithValue("@term", "%" + searchTerm + "%");

                if (con.State != ConnectionState.Open)
                    con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sections.Add(new CourseSection
                        {
                            CourseCode = reader["Course_Code"].ToString(),
                            SectionNumber = Convert.ToInt32(reader["Section_Number"]),
                            Day = reader["Day"].ToString(),
                            StartTime = reader["Start_Time"].ToString(),
                            EndTime = reader["End_Time"].ToString()
                        });
                    }
                }
                con.Close();
            }

            return sections;
        }



        public void RegisterCourse(int studentId, string courseCode, int sectionNumber)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;
                // Insert into Enrolled_In instead of Registered_Courses
                cmd.CommandText = @"
            INSERT INTO Enrolled_In 
                (Student_ID, Course_Code, Section_Number) 
            VALUES 
                (@StudentID, @CourseCode, @SectionNumber)";
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                cmd.Parameters.AddWithValue("@SectionNumber", sectionNumber);

                if (con.State != ConnectionState.Open)
                    con.Open();

                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

    }
}

