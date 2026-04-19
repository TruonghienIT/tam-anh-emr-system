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
                    WHERE u.role = 'Doctor'";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Doctors
                        {
                            Id = (int)reader["id"],
                            UserId = (int)reader["user_id"],
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

        // ================= ADD =================
        public void AddDoctor(Doctors doctor)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    INSERT INTO users (username, password, role, created_at)
                    VALUES (@username, @password, 'Doctor', GETDATE());
                    SELECT SCOPE_IDENTITY();";

                command.Parameters.Add("@username", SqlDbType.VarChar).Value = doctor.Email;
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = "123456";

                int userId = Convert.ToInt32(command.ExecuteScalar());
                command.Parameters.Clear();

                command.CommandText = @"
                    INSERT INTO doctors (user_id, full_name, specialization, phone, email)
                    VALUES (@user_id, @full_name, @specialization, @phone, @email)";

                command.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@full_name", SqlDbType.NVarChar).Value = doctor.FullName;
                command.Parameters.Add("@specialization", SqlDbType.NVarChar).Value = doctor.Specialization ?? "";
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = doctor.Phone;
                command.Parameters.Add("@email", SqlDbType.VarChar).Value = doctor.Email;

                command.ExecuteNonQuery();
            }
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

                command.Parameters.Add("@user_id", SqlDbType.Int).Value = doctor.UserId;
                command.Parameters.Add("@full_name", SqlDbType.NVarChar).Value = doctor.FullName;
                command.Parameters.Add("@specialization", SqlDbType.NVarChar).Value = doctor.Specialization ?? "";
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = doctor.Phone;
                command.Parameters.Add("@email", SqlDbType.VarChar).Value = doctor.Email;

                command.ExecuteNonQuery();
            }
        }

        // ================= DELETE =================
        public void DeleteDoctor(int userId)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "DELETE FROM doctors WHERE user_id=@id";
                command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                command.CommandText = "DELETE FROM users WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
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
                    WHERE u.role = 'Doctor'
                    AND (
                        d.full_name LIKE @key OR 
                        d.phone LIKE @key OR 
                        d.email LIKE @key OR
                        d.specialization LIKE @key
                    )";

                command.Parameters.Add("@key", SqlDbType.NVarChar)
                    .Value = "%" + keyword.Trim() + "%";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Doctors
                        {
                            Id = (int)reader["id"],
                            UserId = (int)reader["user_id"],
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