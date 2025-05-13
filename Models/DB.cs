using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;

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

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                string query = @"
            SELECT 
                S.Day,
                S.Start_Time AS StartTime,
                S.End_Time AS EndTime,
                C.Course_Code,
                C.Course_Name,
                S.Section_Number
            FROM Enrolled_In EI
            JOIN Sections S ON EI.Section_Number = S.Section_Number AND EI.Course_Code = S.Course_Code
            JOIN Courses C ON S.Course_Code = C.Course_Code
            WHERE EI.Student_ID = @studentId
            ORDER BY 
                CASE 
                    WHEN S.Day = 'Saturday' THEN 1
                    WHEN S.Day = 'Sunday' THEN 2
                    WHEN S.Day = 'Monday' THEN 3
                    WHEN S.Day = 'Tuesday' THEN 4
                    WHEN S.Day = 'Wednesday' THEN 5
                    WHEN S.Day = 'Thursday' THEN 6
                    ELSE 7
                END, 
                S.Start_Time;
        ";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Original error: " + ex.Message); // Optional logging
                throw new Exception("Error fetching student timetable", ex);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            return timetable;
        }



        public List<CourseSection> GetSectionsByCourseName(string term)
        {
            var list = new List<CourseSection>();
            using var cmd = new SqlCommand(@"
            SELECT S.Section_Number, S.Course_Code, S.Day, S.Start_Time, S.End_Time
              FROM Sections S
              JOIN Courses  C ON S.Course_Code = C.Course_Code
             WHERE LOWER(C.Course_Name) LIKE LOWER(@t)
                OR LOWER(C.Course_Code) LIKE LOWER(@t)", con);
            cmd.Parameters.AddWithValue("@t", "%" + term + "%");

            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new CourseSection
                {
                    SectionNumber = Convert.ToInt32(r["Section_Number"]),
                    CourseCode = r["Course_Code"].ToString(),
                    Day = r["Day"].ToString(),
                    StartTime = r["Start_Time"].ToString(),
                    EndTime = r["End_Time"].ToString()
                });
            }
            con.Close();
            return list;
        }

     

        public List<RegisteredCourse> GetRegisteredCourses(int sid)
        {
            var list = new List<RegisteredCourse>();
            using var cmd = new SqlCommand(@"
            SELECT E.Course_Code, E.Section_Number,
                   S.Day, S.Start_Time, S.End_Time
              FROM Enrolled_In E
              JOIN Sections    S 
                ON E.Course_Code    = S.Course_Code
               AND E.Section_Number = S.Section_Number
             WHERE E.Student_ID = @sid", con);
            cmd.Parameters.AddWithValue("@sid", sid);

            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new RegisteredCourse
                {
                    CourseCode = r["Course_Code"].ToString(),
                    SectionNumber = Convert.ToInt32(r["Section_Number"]),
                    Day = r["Day"].ToString(),
                    StartTime = r["Start_Time"].ToString(),
                    EndTime = r["End_Time"].ToString()
                });
            }
            con.Close();
            return list;
        }

     
        public bool DropCourse(int studentId, string courseCode, int sectionNumber)
        {
            using var cmd = new SqlCommand(@"
            DELETE FROM Enrolled_In
             WHERE Student_ID    = @sid
               AND Course_Code   = @cc
               AND Section_Number= @sn", con);
            cmd.Parameters.AddWithValue("@sid", studentId);
            cmd.Parameters.AddWithValue("@cc", courseCode);
            cmd.Parameters.AddWithValue("@sn", sectionNumber);

            con.Open();
            int rows = cmd.ExecuteNonQuery();
            con.Close();
            return rows == 1;
        }

        public bool RegisterCourse(int studentId, string courseCode, int sectionNumber)
        {
            
            {
                con.Open();
                string query = "INSERT INTO Enrolled_In (Student_ID, Course_Code, Section_Number) VALUES (@StudentId, @CourseCode, @SectionNumber)";
                using (var command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@CourseCode", courseCode);
                    command.Parameters.AddWithValue("@SectionNumber", sectionNumber);

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0; // Return true if the registration was successful
                    }
                    catch (Exception ex)
                    {
                        // Handle exception (log it if necessary)
                        return false;
                    }
                }
            }
        }

        // Function to remove a course from a student's enrollment
        public bool RemoveCourse(int studentId, string courseCode, int sectionNumber)
        {
            
            {
                con.Open();
                string query = "DELETE FROM Enrolled_In WHERE Student_ID = @StudentId AND Course_Code = @CourseCode AND Section_Number = @SectionNumber";
                using (var command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@CourseCode", courseCode);
                    command.Parameters.AddWithValue("@SectionNumber", sectionNumber);

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0; // Return true if the removal was successful
                    }
                    catch (Exception ex)
                    {
                        // Handle exception (log it if necessary)
                        return false;
                    }
                }
            }
        }

        // Function to get all the registered courses for a student
        public List<SimpleCourse> GetRegisteredCoursesdetails(int studentId)
        {
            var courses = new List<SimpleCourse>();
           
            {
                con.Open();
                string query = "SELECT Course_Code, Section_Number FROM Enrolled_In WHERE Student_ID = @StudentId";
                using (var command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var course = new SimpleCourse
                            {
                                CourseCode = reader["Course_Code"].ToString(),
                                SectionNumber = (int)reader["Section_Number"]
                            };
                            courses.Add(course);
                        }
                    }
                }
            }
            return courses;
        }

        // Function to get all available sections for a given course
        public List<SimpleCourse> GetAvailableSections(string courseCode)
        {
            var availableSections = new List<SimpleCourse>();
          
            {
                con.Open();
                string query = "SELECT Course_Code, Section_Number FROM Sections WHERE Course_Code = @CourseCode";
                using (var command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@CourseCode", courseCode);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var course = new SimpleCourse
                            {
                                CourseCode = reader["Course_Code"].ToString(),
                                SectionNumber = (int)reader["Section_Number"]
                            };
                            availableSections.Add(course);
                        }
                    }
                }
            }
            return availableSections;
        }
        public List<Task> GetTasksByStudent(int studentId)
        {
            var tasks = new List<Task>();

            con.Open();
            string query = @"
        SELECT t.Task_Number, t.Descript
        FROM Tasks t
        INNER JOIN Task_Submissions ts ON t.Task_Number = ts.Task_Number
        WHERE ts.Student_ID = @StudentId";

            using (var command = new SqlCommand(query, con))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var task = new Task
                        {
                            TaskNumber = reader.GetInt32(reader.GetOrdinal("Task_Number")),
                            Title = reader.IsDBNull(reader.GetOrdinal("Descript"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Descript"))
                        };
                        tasks.Add(task);
                    }
                }
            }
            con.Close();

            return tasks;
        }

        public List<TaskSubmission> GetSubmissionsByStudent(int studentId)
        {
            var submissions = new List<TaskSubmission>();
          
            con.Open();

            string query = "SELECT Submission_ID, Student_ID, Task_Number, Submission_link FROM Task_Submissions WHERE Student_ID = @StudentID";
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@StudentID", studentId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                submissions.Add(new TaskSubmission
                {
                    SubmissionId = reader.GetInt32(0),
                    StudentId = reader.GetInt32(1),
                    TaskNumber = reader.GetInt32(2),
                    SubmissionLink = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }
            con.Close();

            return submissions;
        }

        public void SubmitTask(TaskSubmission submission)
        {
            con.Open();

            string query = @"
                IF EXISTS (
                    SELECT 1 FROM Task_Submissions 
                    WHERE Student_ID = @StudentID AND Task_Number = @TaskNumber
                )
                BEGIN
                    UPDATE Task_Submissions
                    SET Submission_link = @Link
                    WHERE Student_ID = @StudentID AND Task_Number = @TaskNumber
                END
                ELSE
                BEGIN
                    INSERT INTO Task_Submissions (Student_ID, Task_Number, Submission_link)
                    VALUES (@StudentID, @TaskNumber, @Link)
                END";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@StudentID", submission.StudentId);
            cmd.Parameters.AddWithValue("@TaskNumber", submission.TaskNumber);
            cmd.Parameters.AddWithValue("@Link", submission.SubmissionLink ?? "");

            cmd.ExecuteNonQuery();
            con.Close();
        }


    }

}
