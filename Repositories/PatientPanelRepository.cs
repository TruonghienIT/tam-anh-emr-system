using System;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    public class PatientPanelRepository : RepositoryBase
    {
        public ObservableCollection<Patients> GetAllPatients()
        {
            var list = new ObservableCollection<Patients>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "SELECT * FROM patients";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Patients
                        {
                            Id = reader["id"].ToString(),
                            UserId = reader["user_id"]?.ToString(),
                            Name = reader["name"].ToString(),
                            Dob = Convert.ToDateTime(reader["dob"]),
                            Gender = reader["gender"].ToString(),
                            Address = reader["address"].ToString(),
                            Phone = reader["phone"].ToString(),
                            Email = reader["email"].ToString(),
                            IdCard = reader["id_card"].ToString(),
                            BloodType = reader["blood_type"].ToString(),
                            Allergies = reader["allergies"].ToString(),
                            EmergencyContactName = reader["emergency_contact_name"].ToString(),
                            EmergencyContactPhone = reader["emergency_contact_phone"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public void AddPatient(Patients patient)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                INSERT INTO patients
                (
                    id,
                    name,
                    dob,
                    gender,
                    address,
                    phone,
                    email,
                    id_card,
                    blood_type,
                    allergies,
                    emergency_contact_name,
                    emergency_contact_phone,
                    created_at
                )
                VALUES
                (
                    'P' + RIGHT('000' + CAST(
                        ISNULL((SELECT MAX(CAST(SUBSTRING(id,2,LEN(id)) AS INT)) FROM patients),0) + 1 AS VARCHAR
                    ),3),

                    @name,
                    @dob,
                    @gender,
                    @address,
                    @phone,
                    @email,
                    @id_card,
                    @blood_type,
                    @allergies,
                    @emergency_contact_name,
                    @emergency_contact_phone,
                    GETDATE()
                )";

                command.Parameters.Add("@name", SqlDbType.NVarChar).Value = patient.Name;
                command.Parameters.Add("@dob", SqlDbType.Date).Value = patient.Dob;
                command.Parameters.Add("@gender", SqlDbType.NVarChar).Value = patient.Gender;
                command.Parameters.Add("@address", SqlDbType.NVarChar).Value = patient.Address;
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = patient.Phone;
                command.Parameters.Add("@email", SqlDbType.VarChar).Value = patient.Email;
                command.Parameters.Add("@id_card", SqlDbType.VarChar).Value = patient.IdCard;
                command.Parameters.Add("@blood_type", SqlDbType.VarChar).Value = patient.BloodType;
                command.Parameters.Add("@allergies", SqlDbType.NVarChar).Value = patient.Allergies;
                command.Parameters.Add("@emergency_contact_name", SqlDbType.NVarChar).Value = patient.EmergencyContactName;
                command.Parameters.Add("@emergency_contact_phone", SqlDbType.VarChar).Value = patient.EmergencyContactPhone;

                command.ExecuteNonQuery();
            }
        }

        public void UpdatePatient(Patients patient)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                UPDATE patients
                SET
                    name=@name,
                    dob=@dob,
                    gender=@gender,
                    address=@address,
                    phone=@phone,
                    email=@email,
                    id_card=@id_card,
                    blood_type=@blood_type,
                    allergies=@allergies,
                    emergency_contact_name=@emergency_contact_name,
                    emergency_contact_phone=@emergency_contact_phone
                WHERE id=@id";

                command.Parameters.Add("@id", SqlDbType.VarChar).Value = patient.Id;
                command.Parameters.Add("@name", SqlDbType.NVarChar).Value = patient.Name;
                command.Parameters.Add("@dob", SqlDbType.Date).Value = patient.Dob;
                command.Parameters.Add("@gender", SqlDbType.NVarChar).Value = patient.Gender;
                command.Parameters.Add("@address", SqlDbType.NVarChar).Value = patient.Address;
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = patient.Phone;
                command.Parameters.Add("@email", SqlDbType.VarChar).Value = patient.Email;
                command.Parameters.Add("@id_card", SqlDbType.VarChar).Value = patient.IdCard;
                command.Parameters.Add("@blood_type", SqlDbType.VarChar).Value = patient.BloodType;
                command.Parameters.Add("@allergies", SqlDbType.NVarChar).Value = patient.Allergies;
                command.Parameters.Add("@emergency_contact_name", SqlDbType.NVarChar).Value = patient.EmergencyContactName;
                command.Parameters.Add("@emergency_contact_phone", SqlDbType.VarChar).Value = patient.EmergencyContactPhone;

                command.ExecuteNonQuery();
            }
        }

        public void DeletePatient(string id)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "DELETE FROM patients WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.VarChar).Value = id;

                command.ExecuteNonQuery();
            }
        }

        public ObservableCollection<Patients> SearchPatients(string keyword)
        {
            var list = new ObservableCollection<Patients>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                SELECT *
                FROM patients
                WHERE
                    name LIKE @key OR
                    phone LIKE @key OR
                    email LIKE @key OR
                    id_card LIKE @key";

                command.Parameters.Add("@key", SqlDbType.NVarChar)
                    .Value = "%" + keyword + "%";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Patients
                        {
                            Id = reader["id"].ToString(),
                            UserId = reader["user_id"]?.ToString(),
                            Name = reader["name"].ToString(),
                            Dob = Convert.ToDateTime(reader["dob"]),
                            Gender = reader["gender"].ToString(),
                            Address = reader["address"].ToString(),
                            Phone = reader["phone"].ToString(),
                            Email = reader["email"].ToString(),
                            IdCard = reader["id_card"].ToString(),
                            BloodType = reader["blood_type"].ToString(),
                            Allergies = reader["allergies"].ToString(),
                            EmergencyContactName = reader["emergency_contact_name"].ToString(),
                            EmergencyContactPhone = reader["emergency_contact_phone"].ToString()
                        });
                    }
                }
            }

            return list;
        }
    }
}