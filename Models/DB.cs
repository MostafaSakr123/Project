using System.Data.SqlClient;
using System.Data;

namespace Project.Models
{
    public class DB
    {

        public SqlConnection con { get; set; }
        public DB()
        {
            string conStr = "Data Source =DESKTOP;Initial Catalog=University; Integrated Security=True;TrustServerCertificate=True";
            con = new SqlConnection(conStr);
        }

        // This fn returns a Tuple<string, string> → (role, id):
        public Tuple<string, string> GetUserRole(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            {
                // Check Admin
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Username FROM Admin WHERE Username = @username AND Password = @password",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    connection.Open();
                    object adminResult = cmd.ExecuteScalar();
                    connection.Close();

                    if (adminResult != null)
                        return Tuple.Create("Admin", adminResult.ToString());
                }

                // Check Professor
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT P.ID FROM Professor P 
            JOIN [User] U ON P.UserID = U.UserID 
            WHERE U.Username = @username AND U.Password = @password",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    connection.Open();
                    object profResult = cmd.ExecuteScalar();
                    connection.Close();

                    if (profResult != null)
                        return Tuple.Create("Professor", profResult.ToString());
                }

                // Check Student
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT S.ID FROM Student S 
            JOIN [User] U ON S.UserID = U.UserID 
            WHERE U.Username = @username AND U.Password = @password",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    connection.Open();
                    object studentResult = cmd.ExecuteScalar();
                    connection.Close();

                    if (studentResult != null)
                        return Tuple.Create("Student", studentResult.ToString());
                }
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

        // Add to DB class
        public bool IsStudentEnrolled(string studentId, string courseCode, string semester)
        {
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = @"SELECT COUNT(*) 
                          FROM Enrolled_In 
                          WHERE Student_ID = @studentId 
                            AND Course_Code = @courseCode
                            AND Semester = @semester";
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@courseCode", courseCode);
                cmd.Parameters.AddWithValue("@semester", semester);

                try
                {
                    connection.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public bool InsertGrade(string semester, string studentId, string courseCode, string grade)
        {
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = @"INSERT INTO Grades (semester, student_id, course_code, letter_grade)
                          VALUES (@semester, @studentId, @courseCode, @grade)";
                cmd.Parameters.AddWithValue("@semester", semester);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@courseCode", courseCode);
                cmd.Parameters.AddWithValue("@grade", grade);

                try
                {
                    connection.Open();
                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
                catch (SqlException ex)
                {
                    // Log error (e.g., Console.WriteLine(ex.Message))
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public DataTable GetProfessorCourses(string professorId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = @"SELECT DISTINCT Course_Code 
                              FROM Sections 
                              WHERE Prof_ID = @profId";
                    cmd.Parameters.AddWithValue("@profId", professorId);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                            connection.Close();
                    }
                }
            }

            return dt;
        }
        public bool UpsertGrade(string semester, string studentId, string courseCode, string grade)
        {
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            {
                string query = @"MERGE INTO Grades AS target
                        USING (VALUES (@semester, @studentId, @courseCode, @grade)) 
                        AS source (semester, student_id, course_code, letter_grade)
                        ON target.semester = source.semester
                        AND target.student_id = source.student_id
                        AND target.course_code = source.course_code
                        WHEN MATCHED THEN
                            UPDATE SET letter_grade = source.letter_grade
                        WHEN NOT MATCHED THEN
                            INSERT (semester, student_id, course_code, letter_grade)
                            VALUES (source.semester, source.student_id, source.course_code, source.letter_grade);";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@semester", semester);
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@courseCode", courseCode);
                    cmd.Parameters.AddWithValue("@grade", grade);

                    try
                    {
                        connection.Open();
                        int affected = cmd.ExecuteNonQuery();
                        return affected > 0;
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine($"SQL Error: {ex.Message}");
                        return false;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        public bool UpdateGrade(string semester, string studentId, string courseCode, string newGrade)
        {
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Grades SET letter_grade = @grade " +
                "WHERE semester = @sem AND student_id = @stuId AND course_code = @cc",
                connection))
            {
                cmd.Parameters.AddWithValue("@sem", semester);
                cmd.Parameters.AddWithValue("@stuId", studentId);
                cmd.Parameters.AddWithValue("@cc", courseCode);
                cmd.Parameters.AddWithValue("@grade", newGrade);

                connection.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }


    }
}
