using System;
using System.Data;
using System.Data.SqlClient;

namespace Project.Models
{
    public class DB
    {
        public SqlConnection con { get; set; }

        public DB()
        {
            string conStr = "Data Source=DESKTOP;Initial Catalog=University;Integrated Security=True;TrustServerCertificate=True";
            con = new SqlConnection(conStr);
        }

        // Authentication
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
                    if (adminResult != null) return Tuple.Create("Admin", adminResult.ToString());
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
                    if (profResult != null) return Tuple.Create("Professor", profResult.ToString());
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
                    if (studentResult != null) return Tuple.Create("Student", studentResult.ToString());
                }
            }
            return Tuple.Create("Invalid", "");
        }

        // Courses
        public DataTable GetProfessorCourses(string professorId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT DISTINCT Course_Code FROM Sections WHERE Prof_ID = @profId", connection))
                {
                    cmd.Parameters.AddWithValue("@profId", professorId);
                    connection.Open();
                    dt.Load(cmd.ExecuteReader());
                }
            }
            return dt;
        }

        // Enrollment Check
        public bool IsStudentEnrolled(int studentId, string courseCode, string semester)
        {
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT COUNT(*) FROM Enrolled_In 
                    WHERE Student_ID = @studentId 
                    AND Course_Code = @courseCode", connection))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@courseCode", courseCode);
                    connection.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        // Grade Management
        public bool UpsertGrade(string semester, int studentId, string courseCode, string grade)
        {
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            {
                string query = @"MERGE Grades AS target
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
                try
                {
                    connection.Open();
                    new SqlCommand(query, connection)
                    {
                        Parameters =
                        {
                            new SqlParameter("@semester", semester),
                            new SqlParameter("@studentId", studentId),
                            new SqlParameter("@courseCode", courseCode),
                            new SqlParameter("@grade", grade)
                        }
                    }.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
        }

        /// <Tasks>
        /// ///////////////////
        /// </summary>
        /// <param name="professorId"></param>
        /// <param name="courseCode"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="deadline"></param>
        /// <param name="totalGrade"></param>
        /// <param name="allowLate"></param>
        //Tasks uploads and management (professor)

        // Fix SaveTaskDraft method (missing parentheses and parameter)
        public void SaveTaskDraft(int professorId, string courseCode, string title,
                        string description, DateTime deadline, int totalGrade, bool allowLate)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open();
                var query = @"INSERT INTO Tasks 
            (Prof_ID, Course_Code, TaskTitle, Description, Deadline, TotalGrade, AllowLateSubmission,PublishedDate, IsDraft)
            VALUES 
            (@profId, @course, @title, @desc, @deadline, @grade, @allowLate,GETDATE(), 1)"; // Fixed syntax

                new SqlCommand(query, connection)
                {
                    Parameters =
            {
                new SqlParameter("@profId", professorId),
                new SqlParameter("@course", courseCode),
                new SqlParameter("@title", title),
                new SqlParameter("@desc", description ?? (object)DBNull.Value),
                new SqlParameter("@deadline", deadline),
                new SqlParameter("@grade", totalGrade),
                new SqlParameter("@allowLate", allowLate)
            }
                }.ExecuteNonQuery();
            }
        }

        // Add new PublishTask overload for draft conversion
        public void PublishTask(int taskId)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open();
                var query = @"UPDATE Tasks 
                     SET IsDraft = 0, 
                         PublishedDate = GETDATE() 
                     WHERE Task_Number = @taskId";

                new SqlCommand(query, connection)
                {
                    Parameters = { new SqlParameter("@taskId", taskId) }
                }.ExecuteNonQuery();
            }
        }

        public void PublishTask(int professorId, string courseCode, string title,
                        string description, DateTime deadline, int totalGrade, bool allowLate)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open();
                var query = @"
            INSERT INTO Tasks 
                (Prof_ID, Course_Code, TaskTitle, Description, Deadline, TotalGrade, AllowLateSubmission, PublishedDate,IsDraft)
            VALUES 
                (@profId, @course, @title, @desc, @deadline, @grade, @allowLate, GETDATE(),0) ";

                new SqlCommand(query, connection)
                {
                    Parameters =
            {
                new SqlParameter("@profId", professorId),
                new SqlParameter("@course", courseCode),
                new SqlParameter("@title", title),
                new SqlParameter("@desc", description ?? (object)DBNull.Value),
                new SqlParameter("@deadline", deadline),
                new SqlParameter("@grade", totalGrade),
                new SqlParameter("@allowLate", allowLate)
            }
                }.ExecuteNonQuery();
            }
        }

        // In DB.cs
        public List<PublishedTask> GetPublishedTasks(int professorId)
        {
            var tasks = new List<PublishedTask>();
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open();
                var query = @"SELECT Task_Number, TaskTitle, Course_Code, PublishedDate 
                     FROM Tasks 
                     WHERE Prof_ID = @profId AND IsDraft = 0";

                var reader = new SqlCommand(query, connection)
                {
                    Parameters = { new SqlParameter("@profId", professorId) }
                }.ExecuteReader();

                while (reader.Read())
                {
                    tasks.Add(new PublishedTask
                    {
                        TaskId = reader.GetInt32(0),
                        TaskTitle = reader.GetString(1),
                        CourseCode = reader.GetString(2),
                        PublishedDate = reader.GetDateTime(3)
                    });
                }
            }
            return tasks;
        }

        // Rename DeleteDraft to DeleteTask for generic use
        public void DeleteTask(int taskId)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open();
                new SqlCommand("DELETE FROM Tasks WHERE Task_Number = @taskId", connection)
                {
                    Parameters = { new SqlParameter("@taskId", taskId) }
                }.ExecuteNonQuery();
            }
        }

        // Add this class to Models/DB.cs
       
        // Get drafts for a professor
        public List<Draft> GetDrafts(int professorId)
        {
            var drafts = new List<Draft>();
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open(); // 👈 Open the connection

                var query = @"
            SELECT Task_Number, TaskTitle, Course_Code 
            FROM Tasks 
            WHERE Prof_ID = @profId AND IsDraft = 1";

                var reader = new SqlCommand(query, connection)
                {
                    Parameters = { new SqlParameter("@profId", professorId) }
                }.ExecuteReader();

                while (reader.Read())
                {
                    drafts.Add(new Draft
                    {
                        TaskId = reader.GetInt32(0),
                        TaskTitle = reader.GetString(1),
                        CourseCode = reader.GetString(2)
                    });
                }
            }
            return drafts;
        }

        // Delete draft
        public void DeleteDraft(int taskId)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open();
                new SqlCommand(
                    "DELETE FROM Tasks WHERE Task_Number = @taskId",
                    connection)
                {
                    Parameters = { new SqlParameter("@taskId", taskId) }
                }.ExecuteNonQuery();
            }
        }

        // In DB.cs
        public int GetDraftCount(int professorId)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open(); 

                var query = @"
            SELECT COUNT(*) 
            FROM Tasks 
            WHERE Prof_ID = @profId AND IsDraft = 1";

                return (int)new SqlCommand(query, connection)
                {
                    Parameters = { new SqlParameter("@profId", professorId) }
                }.ExecuteScalar();
            }
        }
        public int GetTaskCount(int professorId)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open();

                var query = @"
            SELECT COUNT(*) 
            FROM Tasks 
            WHERE Prof_ID = @profId AND IsDraft = 0";

                return (int)new SqlCommand(query, connection)
                {
                    Parameters = { new SqlParameter("@profId", professorId) }
                }.ExecuteScalar();
            }
        }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="professorId"></param>
        /// <returns></returns>
        // In DB.cs
        public List<StudentGrade> GetGradesByProfessor(int professorId)
        {
            var grades = new List<StudentGrade>();
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                connection.Open();
                var query = @"
            SELECT G.student_id, S.Name, G.course_code, G.semester, G.letter_grade
            FROM Grades G
            INNER JOIN Student S ON G.student_id = S.ID
            INNER JOIN Sections Sec ON G.course_code = Sec.Course_Code
            WHERE Sec.Prof_ID = @professorId
            ORDER BY 
                G.course_code,
                CASE G.letter_grade
                    WHEN 'A' THEN 1
                    WHEN 'A-' THEN 2
                    WHEN 'B+' THEN 3
                    WHEN 'B' THEN 4
                    WHEN 'B-' THEN 5
                    WHEN 'C+' THEN 6
                    WHEN 'C' THEN 7
                    WHEN 'C-' THEN 8
                    ELSE 9
                END";

                var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@professorId", professorId);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    grades.Add(new StudentGrade
                    {
                        StudentId = reader.GetInt32(0),
                        StudentName = reader.GetString(1),
                        CourseCode = reader.GetString(2),
                        Semester = reader.GetString(3),
                        LetterGrade = reader.GetString(4)
                    });
                }
            }
            return grades;
        }

        /// ////////////////////////////////////




        // Admin
        public string GetAdminName(int id)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Name FROM Admin WHERE ID = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                object result = cmd.ExecuteScalar();
                con.Close();
                return result?.ToString() ?? "";
            }
        }

    }
}
























public class PublishedTask
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; }
    public string CourseCode { get; set; }
    public DateTime PublishedDate { get; set; }
}


// Add at bottom of DB.cs
public class Draft
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; }
    public string CourseCode { get; set; }

    public DateTime PublishedDate { get; set; }

}
// Add to DB.cs
public class StudentGrade
{
    public int StudentId { get; set; }
    public string StudentName { get; set; }
    public string CourseCode { get; set; }
    public string Semester { get; set; }
    public string LetterGrade { get; set; }

    // For sorting grades A -> F
    public int LetterGradeOrder => LetterGrade switch
    {
        "A" => 1,
        "A-" => 2,
        "B+" => 3,
        "B" => 4,
        "B-" => 5,
        "C+" => 6,
        "C" => 7,
        "C-" => 8,
        _ => 9
    };
}