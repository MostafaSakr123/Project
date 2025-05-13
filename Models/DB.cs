using Project.Pages;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using Dapper;

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
                //Check Visitor
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT v.username
                      FROM Visitor v  
                      WHERE v.Username = @username AND v.Password = @password",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    connection.Open();
                    object visitorResult = cmd.ExecuteScalar();
                    connection.Close();
                    if (visitorResult != null) return Tuple.Create("Visitor", visitorResult.ToString());
                }

            }
            return Tuple.Create("Invalid", "");
        }

        // Courses
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

        // Get course count by major (for pie chart)
        public async Task<Dictionary<string, int>> GetCourseCountByMajor()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    await con.OpenAsync();

                var result = await con.QueryAsync(
                    @"SELECT m.Major_Code, COUNT(c.Course_Code) as Count 
                FROM Major m
                LEFT JOIN Courses c ON m.Major_Code = c.Major_Code
                GROUP BY m.Major_Code");

                var dict = new Dictionary<string, int>();
                foreach (dynamic row in result)
                {
                    dict.Add(row.Major_Code, row.Count);
                }
                return dict;
            }
            finally
            {
                // Keep connection open for potential reuse
            }
        }

        // Get student count by course (for bar chart)
        public async Task<Dictionary<string, Dictionary<string, int>>> GetStudentCountByCourse()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    await con.OpenAsync();

                var result = await con.QueryAsync(
                    @"SELECT 
                    c.Course_Name,
                    'Section ' + CAST(s.Section_Number AS VARCHAR) AS Section,
                    COUNT(e.Student_ID) AS Count
                FROM Courses c
                JOIN Sections s ON c.Course_Code = s.Course_Code
                LEFT JOIN Enrolled_In e ON s.Course_Code = e.Course_Code AND s.Section_Number = e.Section_Number
                GROUP BY c.Course_Name, s.Section_Number");

                var courseDict = new Dictionary<string, Dictionary<string, int>>();
                foreach (dynamic row in result)
                {
                    if (!courseDict.ContainsKey(row.Course_Name))
                    {
                        courseDict[row.Course_Name] = new Dictionary<string, int>();
                    }
                    courseDict[row.Course_Name][row.Section] = row.Count;
                }
                return courseDict;
            }
            finally
            {
                // Keep connection open for potential reuse
            }
        }

        // Get total student count
        public async Task<int> GetTotalStudents()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    await con.OpenAsync();

                return await con.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Student");
            }
            finally
            {
                // Keep connection open for potential reuse
            }
        }

        // Get total course count
        public async Task<int> GetTotalCourses()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    await con.OpenAsync();

                return await con.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Courses");
            }
            finally
            {
                // Keep connection open for potential reuse
            }
        }

        // Get major names for display
        public async Task<List<(string Code, string Name)>> GetMajorNames()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    await con.OpenAsync();

                var result = await con.QueryAsync<(string, string)>(
                    "SELECT Major_Code, Major_Name FROM Major");

                return result.AsList();
            }
            finally
            {
                // Keep connection open for potential reuse
            }
        }

        public bool IsStudentEnrolled(int studentId, string courseCode, int sectionNumber)
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                var sql = @"SELECT COUNT(*) FROM Enrolled_In 
                   WHERE Student_ID = @StudentId 
                   AND Course_Code = @CourseCode
                   AND Section_Number = @SectionNumber";

                int count = con.ExecuteScalar<int>(sql, new
                {
                    StudentId = studentId,
                    CourseCode = courseCode,
                    SectionNumber = sectionNumber
                });

                return count > 0;
            }
            finally
            {
                // Keep connection open if using singleton
            }
        }

        public DataTable GetTableData(string tableName)
        {
            DataTable table = new DataTable();

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                using (var cmd = new SqlCommand($"SELECT * FROM {tableName}", con))
                using (var reader = cmd.ExecuteReader())
                {
                    table.Load(reader);
                }
                return table;
            }
            catch (Exception ex)
            {
                // You might want to log this error
                Console.WriteLine($"Error loading {tableName}: {ex.Message}");
                return table;  // Returns empty table on error
            }
            finally
            {
                // Keep connection open if using singleton
                // Or close it if using per-request: con.Close();
            }
        }

        // Enrollment Check
        public bool IsStudentEnrolled(int studentId, string courseCode)
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="profId"></param>
        /// <param name="courseCode"></param>
        /// <param name="studentId"></param>
        /// <param name="reportDate"></param>
        /// <param name="status"></param>
        public void SaveAttendance(int profId, string courseCode, int studentId,
                           DateTime reportDate, string status)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                var cmd = new SqlCommand(@"
            MERGE Attendance_Report AS target
            USING (VALUES (@profId, @course, @student, @date, @status)) 
                AS source (Prof_ID, Course_Code, Student_ID, Report_Date, Stat)
            ON target.Student_ID = source.Student_ID
                AND target.Course_Code = source.Course_Code
                AND CAST(target.Report_Date AS DATE) = CAST(source.Report_Date AS DATE)
            WHEN MATCHED THEN
                UPDATE SET Stat = source.Stat
            WHEN NOT MATCHED THEN
                INSERT (Prof_ID, Course_Code, Student_ID, Report_Date, Stat)
                VALUES (source.Prof_ID, source.Course_Code, source.Student_ID, 
                        source.Report_Date, source.Stat);",
                    con);

                cmd.Parameters.AddRange(new[]
                {
            new SqlParameter("@profId", profId),
            new SqlParameter("@course", courseCode),
            new SqlParameter("@student", studentId),
            new SqlParameter("@date", reportDate),
            new SqlParameter("@status", status)
        });

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        public DataTable GetProfessorCourses(string professorId)
        {
            DataTable dt = new DataTable();
            using (var connection = new SqlConnection(con.ConnectionString)) // Use local connection
            {
                var cmd = new SqlCommand(
                    "SELECT DISTINCT Course_Code FROM Sections WHERE Prof_ID = @profId",
                    connection // Use local connection, not the global `con`
                );
                cmd.Parameters.AddWithValue("@profId", professorId);
                connection.Open();
                dt.Load(cmd.ExecuteReader());
            } // Connection auto-closed here
            return dt;
        }


        public List<AttendanceRecord> GetAttendanceRecords(string courseCode, DateTime date)
        {
            var records = new List<AttendanceRecord>();
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                var query = @"
            SELECT ar.Student_ID, s.Name, ar.Stat, ar.Report_Date 
            FROM Attendance_Report ar
            INNER JOIN Student s ON ar.Student_ID = s.ID
            WHERE ar.Course_Code = @courseCode
            AND CAST(ar.Report_Date AS DATE) = CAST(@date AS DATE)
            ORDER BY ar.Student_ID";

                var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@courseCode", courseCode);
                cmd.Parameters.AddWithValue("@date", date.Date);

                connection.Open();
                using (var reader = cmd.ExecuteReader()) // Properly initialize reader
                {
                    while (reader.Read())
                    {
                        records.Add(new AttendanceRecord
                        {
                            StudentId = reader.GetInt32(0),
                            StudentName = reader.GetString(1),
                            Status = reader.GetString(2),
                            ReportDate = reader.GetDateTime(3)
                        });
                    }
                }
            }
            return records;
        }
        public List<AttendanceRecord> GetAttendanceRecordsForMultipleCourses(List<string> courseCodes, DateTime date)
        {
            var records = new List<AttendanceRecord>();
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                var query = @"
            SELECT ar.Course_Code, ar.Student_ID, s.Name, ar.Stat, ar.Report_Date 
            FROM Attendance_Report ar
            INNER JOIN Student s ON ar.Student_ID = s.ID
            WHERE ar.Course_Code IN ({0})
            AND CAST(ar.Report_Date AS DATE) = CAST(@date AS DATE)
            ORDER BY ar.Course_Code, ar.Student_ID";

                var parameters = new List<SqlParameter> { new SqlParameter("@date", date.Date) };
                var courseParams = courseCodes.Select((code, index) =>
                    new SqlParameter($"@course{index}", code)).ToList();

                query = string.Format(query, string.Join(",", courseParams.Select(p => p.ParameterName)));
                parameters.AddRange(courseParams);

                var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddRange(parameters.ToArray());

                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(new AttendanceRecord
                        {
                            CourseCode= reader.GetString(0),
                            StudentId = reader.GetInt32(1),
                            StudentName = reader.GetString(2),
                            Status = reader.GetString(3),
                            ReportDate = reader.GetDateTime(4)
                        });
                    }
                }
            }
            return records;
        }
        public void SaveAnnouncement(int professorId, string title, string description, DateTime date)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            {
                var cmd = new SqlCommand(@"
            INSERT INTO Professor_Announcements 
                (ProfessorID, Title, Description, Date)
            VALUES
                (@professorID, @Title, @Description, @Date)",
                    connection);

                cmd.Parameters.AddWithValue("@professorID", professorId)
                    ;
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@Date", date);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Handle exception (log it, throw, etc.)
                    throw new Exception("Failed to save announcement", ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public List<Announcement> GetAnnouncements(int professorId)
        {
            var announcements = new List<Announcement>();
            using (var connection = new SqlConnection(con.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT AnnouncementID, Title, Description, Date 
          FROM Professor_Announcements 
          WHERE ProfessorID = @profId 
          ORDER BY Date DESC", connection))
            {
                cmd.Parameters.AddWithValue("@profId", professorId);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        announcements.Add(new Announcement
                        {
                            AnnouncementID = (int)reader["AnnouncementID"],
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            Date = (DateTime)reader["Date"]
                        });
                    }
                }
            }
            return announcements;
        }

        public bool DeleteAnnouncement(int announcementId, int professorId)
        {
            using (var connection = new SqlConnection(con.ConnectionString))
            using (var cmd = new SqlCommand(
                @"DELETE FROM Professor_Announcements 
          WHERE AnnouncementID = @id 
          AND ProfessorID = @profId", connection))
            {
                cmd.Parameters.AddWithValue("@id", announcementId);
                cmd.Parameters.AddWithValue("@profId", professorId);
                connection.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }


        // In DB.cs
        //public List<TaskSubmission> GetTaskSubmissions(int taskId)
        //{
        //    var submissions = new List<TaskSubmission>();
        //    using (var connection = new SqlConnection(con.ConnectionString))
        //    {
        //        var query = @"SELECT s.Student_ID, st.Name, s.Submission_link 
        //             FROM Task_Submissions s
        //             INNER JOIN Student st ON s.Student_ID = st.ID
        //             WHERE s.Task_Number = @taskId
        //             ORDER BY st.Name";

        //        var cmd = new SqlCommand(query, connection);
        //        cmd.Parameters.AddWithValue("@taskId", taskId);
        //        connection.Open();

        //        using var reader = cmd.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            submissions.Add(new TaskSubmission
        //            {
        //                StudentId = reader.GetInt32(0),
        //                StudentName = reader.GetString(1),
        //                SubmissionLink = reader.IsDBNull(2) ? null : reader.GetString(2)
        //            });
        //        }
        //    }
        //    return submissions;
        //}

        //public PublishedTask GetTaskDetails(int taskId)
        //{
        //    using (var connection = new SqlConnection(con.ConnectionString))
        //    {
        //        var query = @"SELECT TaskTitle, Course_Code, Deadline 
        //             FROM Tasks WHERE Task_Number = @taskId";

        //        var cmd = new SqlCommand(query, connection);
        //        cmd.Parameters.AddWithValue("@taskId", taskId);
        //        connection.Open();

        //        using var reader = cmd.ExecuteReader();
        //        if (reader.Read())
        //        {
        //            return new PublishedTask
        //            {
        //                TaskTitle = reader.GetString(0),
        //                CourseCode = reader.GetString(1),
        //                Deadline = reader.GetDateTime(2)
        //            };
        //        }
        //    }
        //    return null;
        //}
        /// ////////////////////////////////////




        // Admin
       
        ///////Farah new//////Functions/////DBclass
       

        // This fn returns a Tuple<string, string> → (role, id):
       

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
        public void SetVisitor(string username, string password)
        {


            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;

                cmd.CommandText = "INSERT INTO Visitor (Username, Password) VALUES (@Username, @Password)";
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);
                if (con.State != ConnectionState.Open)
                    con.Open();

                cmd.ExecuteNonQuery();
            }
        }
        public DataTable ExecuteReader(string query, Dictionary<string, object> parameters = null)
        {
            using var cmd = new SqlCommand(query, con);
            DataTable dt = new DataTable();
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
            }
            try
            {
                con.Open();
                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                con.Close();
            }
            return dt;
        }
        public DataTable ExecutePage(string query, int ID)
        {
            SqlCommand cmd = new SqlCommand(query, con);
            DataTable page = new DataTable();

            try
            {
                con.Open();
                cmd.Parameters.AddWithValue("@ID", ID);
                page.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                con.Close();
            }
            return page;
        }
        public DataTable ExecuteStringPage(string query, string ID)
        {
            SqlCommand cmd = new SqlCommand(query, con);
            DataTable page = new DataTable();

            try
            {
                con.Open();
                cmd.Parameters.AddWithValue("@CODE", ID);
                page.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                con.Close();
            }

            return page;
        }
        public DataTable Execute(string query, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, con);
            DataTable dt = new DataTable();
            try
            {
                con.Open();
                cmd.Parameters.AddRange(parameters);
                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                con.Close();
            }
            return dt;
        }


    }

}

























public class PublishedTask
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; }
    public string CourseCode { get; set; }
    public DateTime PublishedDate { get; set; }
    public DateTime Deadline { get; set; } // Add this

}


// Add at bottom of DB.cs
public class Draft
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; }
    public string CourseCode { get; set; }

    public DateTime PublishedDate { get; set; }

}
public class AttendanceRecord
{
    public string CourseCode { get; set; } // Add this
    public int StudentId { get; set; }
    public string StudentName { get; set; }
    public string Status { get; set; }
    public DateTime ReportDate { get; set; }
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

// Add to DB.cs


