using System.Data.SqlClient;
using System.Data;

namespace Project.Models
{
    public class DB
    {

        public SqlConnection con { get; set; }
        public DB()
        {
            string conStr = "Data Source =MOHAMED-HANY;Initial Catalog=University; Integrated Security=True;TrustServerCertificate=True";
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

                // Check Visitor
                cmd.CommandText = @"SELECT v.username
                            FROM Visitor v  
                            WHERE v.Username = @username AND v.Password = @password";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                object visitorResult = cmd.ExecuteScalar();
                if (visitorResult != null)
                    return Tuple.Create("Visitor", visitorResult.ToString());
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
        public void SetVisitor(string username, string password)
        {
            // Validate inputs


            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;

                // Use parameterized query to prevent SQL injection
                cmd.CommandText = "INSERT INTO Visitor (Username, Password) VALUES (@Username, @Password)";

                // Add parameters
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                // Open connection if not already open
                if (con.State != ConnectionState.Open)
                    con.Open();

                // Execute the query
                cmd.ExecuteNonQuery();
            }
        }
        public DataTable ExecuteReader(string query)
        {
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

    }
    
}
