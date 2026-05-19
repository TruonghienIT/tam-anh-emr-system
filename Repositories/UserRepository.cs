using System;
using System.Collections.Generic;
using System.Net;
using TamAnh_EMR_System.Model;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TamAnh_EMR_System.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {
        // ================= ADD =================
        public void Add(Users user)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    INSERT INTO [Users]
                    (username, password, role, created_at, updated_at)
                    VALUES
                    (@username, @password, @role, GETDATE(), GETDATE())";

                command.Parameters.Add("@username", SqlDbType.VarChar).Value = user.Username;
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = user.Password;
                command.Parameters.Add("@role", SqlDbType.VarChar).Value = user.Role;

                command.ExecuteNonQuery();
            }
        }

        // ================= AUTH =================
        public Users AuthenticateUser(NetworkCredential credential)
        {
            Users user = null;

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
SELECT 
    u.*,
    r.id AS receptionist_id
FROM users u
LEFT JOIN receptionists r
    ON u.id = r.user_id
WHERE username = @username
AND password = @password";

                command.Parameters.Add("@username", SqlDbType.VarChar).Value = credential.UserName;
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = credential.Password;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = MapUser(reader);
                    }
                }
            }

            return user;
        }

        // ================= EDIT =================
        public void Edit(Users user)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    UPDATE [Users]
                    SET username = @username,
                        password = @password,
                        role = @role,
                        updated_at = GETDATE()
                    WHERE id = @id";

                command.Parameters.Add("@id", SqlDbType.VarChar).Value = user.Id;
                command.Parameters.Add("@username", SqlDbType.VarChar).Value = user.Username;
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = user.Password;
                command.Parameters.Add("@role", SqlDbType.VarChar).Value = user.Role;

                command.ExecuteNonQuery();
            }
        }

        // ================= GET ALL =================
        public IEnumerable<Users> GetByAll()
        {
            var list = new List<Users>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "SELECT * FROM [Users]";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapUser(reader));
                    }
                }
            }

            return list;
        }

        // ================= GET BY ID =================
        public Users GetById(int id)
        {
            Users user = null;

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "SELECT * FROM [Users] WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = MapUser(reader);
                    }
                }
            }

            return user;
        }

        // ================= GET BY USERNAME =================
        public Users GetByUsername(string username)
        {
            Users user = null;

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "SELECT * FROM [Users] WHERE username=@username";
                command.Parameters.Add("@username", SqlDbType.VarChar).Value = username;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = MapUser(reader);
                    }
                }
            }

            return user;
        }

        // ================= DELETE =================
        public void Remove(int id)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "DELETE FROM [Users] WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                command.ExecuteNonQuery();
            }
        }

        // ================= MAPPING CHUẨN =================
        private Users MapUser(SqlDataReader reader)
        {
            return new Users
            {
                Id = reader["id"].ToString(),
                Username = reader["username"].ToString(),
                Password = reader["password"].ToString(),
                Role = reader["role"].ToString(),
                CreatedDate = reader["created_at"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(reader["created_at"]),

                UpdatedDate = reader["updated_at"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(reader["updated_at"])
            };
        }

        public Users GetByEmail(string email)
        {
            Users user = null;

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
            SELECT u.*
            FROM Users u
            LEFT JOIN doctors d ON u.id = d.user_id
            LEFT JOIN receptionists r ON u.id = r.user_id
            LEFT JOIN patients p ON u.id = p.user_id
            WHERE d.email = @email 
               OR r.email = @email 
               OR p.email = @email";

                command.Parameters.Add("@email", SqlDbType.VarChar).Value = email;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = MapUser(reader);
                    }
                }
            }

            return user;
        }
    }
}