using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    public class ReceptionistPanelRepository : RepositoryBase
    {
        // ================= GET ALL =================
        public ObservableCollection<Receptionists> GetAllReceptionists()
        {
            var list = new ObservableCollection<Receptionists>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    SELECT d.*
                    FROM receptionists d
                    INNER JOIN users u ON d.user_id = u.id
                    WHERE u.role = 'receptionist'";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Receptionists
                        {
                            Id = reader["id"].ToString(),
                            UserId = reader["user_id"].ToString(),
                            FullName = reader["full_name"].ToString(),
                            Phone = reader["phone"].ToString(),
                            Email = reader["email"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        // ================= CREATE USER =================
        public (string username, string password, string userId) CreateUser(string email)
        {
            string username = email.Split('@')[0]; ;
            string password = Guid.NewGuid().ToString("N").Substring(0, 8);

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                INSERT INTO users (id, username, password, role, created_at)
                OUTPUT INSERTED.id
                VALUES (
                    'U' + RIGHT('000' + CAST(
                        ISNULL((SELECT MAX(CAST(SUBSTRING(id,2,LEN(id)) AS INT)) FROM users),0) + 1 AS VARCHAR
                    ),3),
                    @username,
                    @password,
                    'receptionist',
                    GETDATE()
                )";

                command.Parameters.Add("@username", SqlDbType.VarChar).Value = username;
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = password;

                string userId = command.ExecuteScalar().ToString();

                return (username, password, userId);
            }
        }

        // ================= ADD RECEPTIONIST =================
        public (string username, string password) AddReceptionist(Receptionists receptionist)
        {
            var account = CreateUser(receptionist.Email);

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                INSERT INTO receptionists (id, user_id, full_name, phone, email)
                VALUES (
                    'R' + RIGHT('000' + CAST(
                        ISNULL((SELECT MAX(CAST(SUBSTRING(id,2,LEN(id)) AS INT)) FROM receptionists),0) + 1 AS VARCHAR
                    ),3),
                    @user_id,
                    @full_name,
                    @phone,
                    @email
                )";

                command.Parameters.Add("@user_id", SqlDbType.VarChar).Value = account.userId;
                command.Parameters.Add("@full_name", SqlDbType.NVarChar).Value = receptionist.FullName;
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = receptionist.Phone;
                command.Parameters.Add("@email", SqlDbType.VarChar).Value = receptionist.Email;

                command.ExecuteNonQuery();
            }

            return (account.username, account.password);
        }

        // ================= UPDATE =================
        public void UpdateReceptionist(Receptionists receptionist)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    UPDATE receptionists
                    SET full_name=@full_name,
                        phone=@phone,
                        email=@email
                    WHERE user_id=@user_id";

                command.Parameters.Add("@user_id", SqlDbType.VarChar).Value = receptionist.UserId;
                command.Parameters.Add("@full_name", SqlDbType.NVarChar).Value = receptionist.FullName;
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = receptionist.Phone;
                command.Parameters.Add("@email", SqlDbType.VarChar).Value = receptionist.Email;

                command.ExecuteNonQuery();
            }
        }

        // ================= DELETE =================
        public void DeleteReceptionist(string userId)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "DELETE FROM receptionists WHERE user_id=@id";
                command.Parameters.Add("@id", SqlDbType.VarChar).Value = userId;
                command.ExecuteNonQuery();

                command.Parameters.Clear();

                command.CommandText = "DELETE FROM users WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.VarChar).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        // ================= SEARCH =================
        public ObservableCollection<Receptionists> SearchReceptionists(string keyword)
        {
            var list = new ObservableCollection<Receptionists>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    SELECT d.*
                    FROM receptionists d
                    INNER JOIN users u ON d.user_id = u.id
                    WHERE u.role = 'receptionist'
                    AND (
                        d.full_name LIKE @key OR 
                        d.phone LIKE @key OR 
                        d.email LIKE @key 
                    )";

                command.Parameters.Add("@key", SqlDbType.NVarChar)
                    .Value = "%" + keyword + "%";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Receptionists
                        {
                            Id = reader["id"].ToString(),
                            UserId = reader["user_id"].ToString(),
                            FullName = reader["full_name"].ToString(),
                            Phone = reader["phone"].ToString(),
                            Email = reader["email"].ToString()
                        });
                    }
                }
            }

            return list;
        }
    }
}
