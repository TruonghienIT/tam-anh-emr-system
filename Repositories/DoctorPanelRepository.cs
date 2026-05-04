using System;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    public class DoctorPanelRepository : RepositoryBase
    {
        // ================= GET ALL =================
        public ObservableCollection<Doctors> GetAllDoctors()
        {
            var list = new ObservableCollection<Doctors>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    SELECT d.*
                    FROM doctors d
                    INNER JOIN users u ON d.user_id = u.id
                    WHERE u.role = 'doctor'";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Doctors
                        {
                            Id = reader["id"].ToString(),
                            UserId = reader["user_id"].ToString(),
                            FullName = reader["full_name"].ToString(),
                            Specialization = reader["specialization"]?.ToString(),
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
                    'doctor',
                    GETDATE()
                )";

                command.Parameters.Add("@username", SqlDbType.VarChar).Value = username;
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = password;

                string userId = command.ExecuteScalar().ToString();

                return (username, password, userId);
            }
        }

        // ================= ADD DOCTOR =================
        public (string username, string password) AddDoctor(Doctors doctor)
        {
            var account = CreateUser(doctor.Email);

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                INSERT INTO doctors (id, user_id, full_name, specialization, phone, email)
                VALUES (
                    'D' + RIGHT('000' + CAST(
                        ISNULL((SELECT MAX(CAST(SUBSTRING(id,2,LEN(id)) AS INT)) FROM doctors),0) + 1 AS VARCHAR
                    ),3),
                    @user_id,
                    @full_name,
                    @specialization,
                    @phone,
                    @email
                )";

                command.Parameters.Add("@user_id", SqlDbType.VarChar).Value = account.userId;
                command.Parameters.Add("@full_name", SqlDbType.NVarChar).Value = doctor.FullName;
                command.Parameters.Add("@specialization", SqlDbType.NVarChar).Value = doctor.Specialization ?? "";
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = doctor.Phone;
                command.Parameters.Add("@email", SqlDbType.VarChar).Value = doctor.Email;

                command.ExecuteNonQuery();
            }

            return (account.username, account.password);
        }

        // ================= UPDATE =================
        public void UpdateDoctor(Doctors doctor)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    UPDATE doctors
                    SET full_name=@full_name,
                        specialization=@specialization,
                        phone=@phone,
                        email=@email
                    WHERE user_id=@user_id";

                command.Parameters.Add("@user_id", SqlDbType.VarChar).Value = doctor.UserId;
                command.Parameters.Add("@full_name", SqlDbType.NVarChar).Value = doctor.FullName;
                command.Parameters.Add("@specialization", SqlDbType.NVarChar).Value = doctor.Specialization ?? "";
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = doctor.Phone;
                command.Parameters.Add("@email", SqlDbType.VarChar).Value = doctor.Email;

                command.ExecuteNonQuery();
            }
        }

        // ================= DELETE =================
        public void DeleteDoctor(string userId)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "DELETE FROM doctors WHERE user_id=@id";
                command.Parameters.Add("@id", SqlDbType.VarChar).Value = userId;
                command.ExecuteNonQuery();

                command.Parameters.Clear();

                command.CommandText = "DELETE FROM users WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.VarChar).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        // ================= SEARCH =================
        public ObservableCollection<Doctors> SearchDoctors(string keyword)
        {
            var list = new ObservableCollection<Doctors>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    SELECT d.*
                    FROM doctors d
                    INNER JOIN users u ON d.user_id = u.id
                    WHERE u.role = 'doctor'
                    AND (
                        d.full_name LIKE @key OR 
                        d.phone LIKE @key OR 
                        d.email LIKE @key OR
                        d.specialization LIKE @key
                    )";

                command.Parameters.Add("@key", SqlDbType.NVarChar)
                    .Value = "%" + keyword + "%";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Doctors
                        {
                            Id = reader["id"].ToString(),
                            UserId = reader["user_id"].ToString(),
                            FullName = reader["full_name"].ToString(),
                            Specialization = reader["specialization"]?.ToString(),
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