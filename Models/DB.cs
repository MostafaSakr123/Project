using System.Data.SqlClient;
using System.Data;
using Project.Pages;

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


        // Add these methods to your existing DB class
        public bool ProfessorExists(int professorId)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Professor WHERE ID = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", professorId);
                if (con.State != ConnectionState.Open)
                    con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public bool MajorExists(string majorCode)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Major WHERE Major_Code = @code", con))
            {
                cmd.Parameters.AddWithValue("@code", majorCode);
                if (con.State != ConnectionState.Open)
                    con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public bool CreateCourse(string courseCode, string courseName, string majorCode,
                                string adminUsername, int numberOfSections)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Courses (Course_Code, Course_Name, Major_Code, 
              Admin_username, Number_of_Sections)
              VALUES (@code, @name, @major, @admin, @sections)", con))
                {
                    cmd.Parameters.AddWithValue("@code", courseCode);
                    cmd.Parameters.AddWithValue("@name", courseName);
                    cmd.Parameters.AddWithValue("@major", majorCode);
                    cmd.Parameters.AddWithValue("@admin", adminUsername);
                    cmd.Parameters.AddWithValue("@sections", numberOfSections);

                    if (con.State != ConnectionState.Open)
                        con.Open();

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                con.Close();
            }
        }

        public List<Course> GetAllCourses()
        {
            var courses = new Dictionary<string, Course>();
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT c.Course_Name AS CourseName, 
                     c.Course_Code AS CourseCode, 
                     c.Major_Code AS MajorCode, 
                     c.Number_of_Sections AS NumberOfSections,
                     s.Section_Number AS SectionNumber, 
                     s.Day, 
                     s.Start_Time AS StartTime, 
                     s.End_Time AS EndTime, 
                     s.Prof_ID AS ProfessorId,
                     p.Name AS ProfessorName
              FROM Courses c
              LEFT JOIN Sections s ON c.Course_Code = s.Course_Code
              LEFT JOIN Professor p ON s.Prof_ID = p.ID
              ORDER BY c.Course_Code, s.Section_Number", con))
            {
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

                        string courseCode = reader["CourseCode"].ToString();
                        if (!courses.ContainsKey(courseCode))
                        {
                            courses[courseCode] = new Course
                            {
                                CourseName = reader["CourseName"].ToString(),
                                CourseCode = courseCode,
                                MajorCode = reader["MajorCode"].ToString(),
                                NumberOfSections = (int)reader["NumberOfSections"]
                            };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("SectionNumber")))
                        {
                            // Parse the start and end time as TimeSpan
                            TimeSpan startTime = TimeSpan.Parse(reader["StartTime"].ToString());
                            TimeSpan endTime = TimeSpan.Parse(reader["EndTime"].ToString());

                            courses[courseCode].Sections.Add(new Section
                            {
                                SectionNumber = (int)reader["SectionNumber"],
                                Day = reader["Day"].ToString(),
                                StartTime = startTime,  // Assign the TimeSpan to StartTime
                                EndTime = endTime,      // Assign the TimeSpan to EndTime
                                ProfID = (int)reader["ProfessorId"],
                                ProfessorName = reader["ProfessorName"].ToString()
                            });
                        }

                    }
                }
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            return courses.Values.ToList();
        }

        public bool DeleteSection(string courseCode, int sectionNumber)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Sections WHERE Course_Code = @courseCode AND Section_Number = @sectionNumber", con))
                {
                    cmd.Parameters.AddWithValue("@courseCode", courseCode);
                    cmd.Parameters.AddWithValue("@sectionNumber", sectionNumber);
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException)
            {
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        public bool DeleteCourse(string courseCode)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Sections WHERE Course_Code = @code; " +
                    "DELETE FROM Courses WHERE Course_Code = @code", con))
                {
                    cmd.Parameters.AddWithValue("@code", courseCode);
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException)
            {
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        public Section GetSection(string courseCode, int sectionNumber)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT * FROM Sections WHERE Course_Code = @courseCode AND Section_Number = @sectionNumber", con))
            {
                cmd.Parameters.AddWithValue("@courseCode", courseCode);
                cmd.Parameters.AddWithValue("@sectionNumber", sectionNumber);

                if (con.State != ConnectionState.Open)
                    con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Section
                        {
                            CourseCode = reader["Course_Code"].ToString(),
                            SectionNumber = (int)reader["Section_Number"],
                            Day = reader["Day"].ToString(),
                            StartTime = TimeSpan.Parse(reader["Start_Time"].ToString()),
                            EndTime = TimeSpan.Parse(reader["End_Time"].ToString()),
                            ProfID = (int)reader["Prof_ID"]
                        };
                    }
                }
            }
            return null;
        }

        public bool UpdateSection(Section section)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Sections
              SET Day = @day, 
                  Start_Time = @startTime, 
                  End_Time = @endTime, 
                  Prof_ID = @profID
              WHERE Course_Code = @courseCode AND Section_Number = @sectionNumber", con))
                {
                    cmd.Parameters.AddWithValue("@courseCode", section.CourseCode);
                    cmd.Parameters.AddWithValue("@sectionNumber", section.SectionNumber);
                    cmd.Parameters.AddWithValue("@day", section.Day);
                    cmd.Parameters.AddWithValue("@startTime", section.StartTime);
                    cmd.Parameters.AddWithValue("@endTime", section.EndTime);
                    cmd.Parameters.AddWithValue("@profID", section.ProfID);

                    if (con.State != ConnectionState.Open)
                        con.Open();

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                // Log the error for debugging
                Console.WriteLine($"SQL Error updating section: {ex.Message}");
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        public List<Student> GetStudentsBySection(string courseCode, int sectionNumber)
        {
            var students = new List<Student>();
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT s.ID, s.Name
              FROM Student s
              JOIN Enrolled_In ei ON s.ID = ei.Student_ID
              WHERE ei.Course_Code = @courseCode AND ei.Section_Number = @sectionNumber", con))
            {
                cmd.Parameters.AddWithValue("@courseCode", courseCode);
                cmd.Parameters.AddWithValue("@sectionNumber", sectionNumber);
                if (con.State != ConnectionState.Open)
                    con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            ID = (int)reader["ID"],
                            Name = reader["Name"].ToString()
                        });
                    }
                }
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            return students;
        }

        public bool DeleteStudentFromSection(int studentID, string courseCode, int sectionNumber)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Enrolled_In WHERE Student_ID = @studentID AND Course_Code = @courseCode AND Section_Number = @sectionNumber", con))
                {
                    cmd.Parameters.AddWithValue("@studentID", studentID);
                    cmd.Parameters.AddWithValue("@courseCode", courseCode);
                    cmd.Parameters.AddWithValue("@sectionNumber", sectionNumber);
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException)
            {
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        public List<Professor> GetAllProfessors()
        {
            var professors = new List<Professor>();
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ID, Name FROM Professor ORDER BY Name", con))
            {
                if (con.State != ConnectionState.Open)
                    con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        professors.Add(new Professor
                        {
                            ID = (int)reader["ID"],
                            Name = reader["Name"].ToString()
                        });
                    }
                }
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            return professors;
        }

        public string GetProfessorName(int profID)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Name FROM Professor WHERE ID = @profID", con))
            {
                cmd.Parameters.AddWithValue("@profID", profID);
                if (con.State != ConnectionState.Open)
                    con.Open();
                var result = cmd.ExecuteScalar()?.ToString();
                con.Close();
                return result ?? "No professor assigned";
            }
        }

        public bool IsProfessorValid(int professorID, string professorName)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Professor WHERE ID = @professorID AND Name = @professorName", con))
            {
                cmd.Parameters.AddWithValue("@professorID", professorID);
                cmd.Parameters.AddWithValue("@professorName", professorName);

                if (con.State != ConnectionState.Open)
                    con.Open();

                var result = cmd.ExecuteScalar();
                return (int)result > 0;  // Return true if both the ID and Name match, false otherwise
            }
        }

        public bool EnrollStudent(int studentId, string courseCode, int sectionNumber)
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand command = new SqlCommand(
                    "INSERT INTO Enrolled_In (Student_ID, Course_Code, Section_Number) VALUES (@StudentID, @CourseCode, @SectionNumber)", con);

                command.Parameters.AddWithValue("@StudentID", studentId);
                command.Parameters.AddWithValue("@CourseCode", courseCode);
                command.Parameters.AddWithValue("@SectionNumber", sectionNumber);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }



        public bool DoesStudentExist(int studentId)
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Student WHERE ID = @ID", con);
                command.Parameters.AddWithValue("@ID", studentId);

                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }


        public bool CreateProfessorAccount(string username, string password, int professorId, string name, string majorCode)
        {
            string connectionString = "Data Source=DESKTOP-9E43EQM;Initial Catalog=University;Integrated Security=True;TrustServerCertificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. First check if user exists
                    int userId;
                    string getUserSql = "SELECT UserId FROM [User] WHERE Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(getUserSql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        object result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            // 2. If user doesn't exist, create it
                            string createUserSql = "INSERT INTO [User] (Username, Password) VALUES (@Username, @Password); SELECT SCOPE_IDENTITY()";

                            using (SqlCommand createCmd = new SqlCommand(createUserSql, connection, transaction))
                            {
                                createCmd.Parameters.AddWithValue("@Username", username);
                                createCmd.Parameters.AddWithValue("@Password", password);
                                userId = Convert.ToInt32(createCmd.ExecuteScalar());
                            }
                        }
                        else
                        {
                            userId = (int)result;
                        }
                    }

                    // 3. Insert professor record
                    string insertProfessorSql = "INSERT INTO Professor (UserID, ID, Name, Major_Code) VALUES (@UserID, @ProfessorId, @Name, @MajorCode)";

                    using (SqlCommand cmd = new SqlCommand(insertProfessorSql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@ProfessorId", professorId);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@MajorCode", majorCode);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception("Failed to insert professor record");
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error creating professor account: {ex.Message}");
                    return false;
                }
            }
        }

        public bool CreateStudentAccount(string username, string password, int studentId, string name, string majorCode)
        {
            const string connectionString = "Data Source=DESKTOP-9E43EQM;Initial Catalog=University;Integrated Security=True;TrustServerCertificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. First check if user exists
                    int userId;
                    string getUserSql = "SELECT UserId FROM [User] WHERE Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(getUserSql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        object result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            // 2. If user doesn't exist, create it
                            string createUserSql = "INSERT INTO [User] (Username, Password) VALUES (@Username, @Password); SELECT SCOPE_IDENTITY()";

                            using (SqlCommand createCmd = new SqlCommand(createUserSql, connection, transaction))
                            {
                                createCmd.Parameters.AddWithValue("@Username", username);
                                createCmd.Parameters.AddWithValue("@Password", password);
                                userId = Convert.ToInt32(createCmd.ExecuteScalar());
                            }
                        }
                        else
                        {
                            userId = (int)result;
                        }
                    }

                    // 3. Insert student record
                    string insertStudentSql = @"
                INSERT INTO Student (ID, Name, Major_Code, UserID) 
                VALUES (@StudentId, @Name, @MajorCode, @UserID)";

                    using (SqlCommand cmd = new SqlCommand(insertStudentSql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", studentId);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@MajorCode", majorCode);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception("Failed to insert student record");
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error creating student account: {ex.Message}");
                    return false;
                }
            }
        }

        public bool CreateAnnouncement(string title, string description, string adminUsername)
        {
            const string connectionString = "Data Source=DESKTOP-9E43EQM;Initial Catalog=University;Integrated Security=True;TrustServerCertificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string sql = @"
                INSERT INTO Announcements (Time, Admin_Username, Description)
                VALUES (@Time, @AdminUsername, @Description)";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Time", DateTime.Now);
                        cmd.Parameters.AddWithValue("@AdminUsername", string.IsNullOrEmpty(adminUsername) ? DBNull.Value : adminUsername);
                        cmd.Parameters.AddWithValue("@Description", $"{title}\n\n{description}");

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating announcement: {ex.Message}");
                    return false;
                }
            }

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

