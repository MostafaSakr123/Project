using System.Data.SqlClient;
using System.Data;
using Project.Pages;
using Dapper;

namespace Project.Models
{
    public class DB
    {

        public SqlConnection con { get; set; }
        public DB()
        {
            string conStr = "Data Source =DESKTOP-9E43EQM;Initial Catalog=University; Integrated Security=True;TrustServerCertificate=True";
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


    }
}
