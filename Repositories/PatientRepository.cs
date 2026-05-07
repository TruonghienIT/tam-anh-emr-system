using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    /// <summary>
    /// ADO.NET implementation of IPatientRepository.
    /// Inherits RepositoryBase for connection string management.
    /// 
    /// Patient ID format: BN000001, BN000002, ...
    /// All write operations accept SqlConnection+SqlTransaction to participate
    /// in the service-level transaction.
    /// </summary>
    public class PatientRepository : RepositoryBase, IPatientRepository
    {
        // ================= ADD (standalone — self-managed connection) =================
        /// <summary>
        /// Inserts a new patient with automatic ID generation.
        /// Manages its own connection and transaction internally.
        /// Use this for standalone patient registration (not part of a larger transaction).
        /// </summary>
        public async Task<Patients> AddAsync(Patients patient)
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                using (var txn = (SqlTransaction)await conn.BeginTransactionAsync())
                {
                    try
                    {
                        var result = await AddAsync(patient, conn, txn);
                        await txn.CommitAsync();
                        return result;
                    }
                    catch
                    {
                        await txn.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // ================= ADD (with external connection/transaction) =================
        public async Task<Patients> AddAsync(Patients patient, SqlConnection conn, SqlTransaction txn)
        {
            // Generate next ID within the same transaction
            patient.Id = await GenerateNextIdAsync(conn, txn);

            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = txn;

                cmd.CommandText = @"
                    INSERT INTO patients 
                    (id, user_id, name, dob, gender, address, phone, email, 
                     id_card, blood_type, allergies, emergency_contact_name, 
                     emergency_contact_phone, created_at)
                    VALUES 
                    (@id, @user_id, @name, @dob, @gender, @address, @phone, @email,
                     @id_card, @blood_type, @allergies, @emergency_contact_name,
                     @emergency_contact_phone, GETDATE())";

                cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = patient.Id;
                cmd.Parameters.Add("@user_id", SqlDbType.VarChar).Value = (object)patient.UserId ?? DBNull.Value;
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = patient.Name ?? "";
                cmd.Parameters.Add("@dob", SqlDbType.Date).Value = (object)patient.Dob ?? DBNull.Value;
                cmd.Parameters.Add("@gender", SqlDbType.NVarChar, 10).Value = patient.Gender ?? "";
                cmd.Parameters.Add("@address", SqlDbType.NVarChar, 255).Value = (object)patient.Address ?? DBNull.Value;
                cmd.Parameters.Add("@phone", SqlDbType.VarChar, 15).Value = patient.Phone ?? "";
                cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = (object)patient.Email ?? DBNull.Value;
                cmd.Parameters.Add("@id_card", SqlDbType.VarChar, 20).Value = (object)patient.IdCard ?? DBNull.Value;
                cmd.Parameters.Add("@blood_type", SqlDbType.VarChar, 5).Value = (object)patient.BloodType ?? DBNull.Value;
                cmd.Parameters.Add("@allergies", SqlDbType.NVarChar, 500).Value = (object)patient.Allergies ?? DBNull.Value;
                cmd.Parameters.Add("@emergency_contact_name", SqlDbType.NVarChar, 100).Value = (object)patient.EmergencyContactName ?? DBNull.Value;
                cmd.Parameters.Add("@emergency_contact_phone", SqlDbType.VarChar, 15).Value = (object)patient.EmergencyContactPhone ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync();
            }

            return patient;
        }

        // ================= FIND EXISTING =================
        public async Task<Patients> FindExistingAsync(string idCard, string phone, DateTime? dob, string name)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                await conn.OpenAsync();
                cmd.Connection = conn;

                // Priority 1: Match by id_card (unique identifier)
                if (!string.IsNullOrWhiteSpace(idCard))
                {
                    cmd.CommandText = "SELECT * FROM patients WHERE id_card = @id_card";
                    cmd.Parameters.Add("@id_card", SqlDbType.VarChar, 20).Value = idCard;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapPatient(reader);
                    }
                }

                // Priority 2: Match by phone + dob + name combo
                if (!string.IsNullOrWhiteSpace(phone) && dob.HasValue && !string.IsNullOrWhiteSpace(name))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                        SELECT * FROM patients 
                        WHERE phone = @phone AND dob = @dob AND name = @name";
                    cmd.Parameters.Add("@phone", SqlDbType.VarChar, 15).Value = phone;
                    cmd.Parameters.Add("@dob", SqlDbType.Date).Value = dob.Value;
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapPatient(reader);
                    }
                }
            }

            return null;
        }

        // ================= EXISTS BY PHONE =================
        public async Task<bool> ExistsByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                await conn.OpenAsync();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT COUNT(*) FROM patients WHERE phone = @phone";
                cmd.Parameters.Add("@phone", SqlDbType.VarChar, 20).Value = phone.Trim();

                var count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        // ================= SEARCH =================
        public async Task<List<Patients>> SearchAsync(string keyword)
        {
            var list = new List<Patients>();

            if (string.IsNullOrWhiteSpace(keyword))
                return list;

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                await conn.OpenAsync();
                cmd.Connection = conn;

                cmd.CommandText = @"
                    SELECT TOP 20 *
                        FROM patients
                        WHERE
                            name LIKE @key
                            OR phone LIKE @key
                            OR id LIKE @key
                            OR id_card LIKE @key
                            OR CONVERT(varchar, dob, 103) LIKE @key
                        ORDER BY created_at DESC";

                cmd.Parameters.Add("@key", SqlDbType.NVarChar, 200).Value = "%" + keyword + "%";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(MapPatient(reader));
                    }
                }
            }

            return list;
        }

        // ================= GET ALL =================
        public async Task<List<Patients>> GetAllAsync()
        {
            var list = new List<Patients>();

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                await conn.OpenAsync();
                cmd.Connection = conn;

                cmd.CommandText = "SELECT TOP 100 * FROM patients ORDER BY created_at DESC";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(MapPatient(reader));
                    }
                }
            }

            return list;
        }

        // ================= GET BY ID =================
        public async Task<Patients> GetByIdAsync(string patientId)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                await conn.OpenAsync();
                cmd.Connection = conn;

                cmd.CommandText = "SELECT * FROM patients WHERE id = @id";
                cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = patientId;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return MapPatient(reader);
                }
            }

            return null;
        }

        // ================= GENERATE NEXT ID (standalone) =================
        public async Task<string> GenerateNextIdAsync()
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();

                return await GenerateNextIdAsync(conn, null);
            }
        }

        // ================= GENERATE NEXT ID (transaction) =================
        public async Task<string> GenerateNextIdAsync(SqlConnection conn, SqlTransaction txn)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = txn;

                cmd.CommandText = @"
            SELECT ISNULL(MAX(CAST(SUBSTRING(id, 3, 6) AS INT)), 0) + 1
            FROM patients";

                int nextNum = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                return $"BN{nextNum:D6}";
            }
        }

        // ================= MAPPING =================
        private Patients MapPatient(SqlDataReader reader)
        {
            return new Patients
            {
                Id = reader["id"]?.ToString(),
                UserId = reader["user_id"]?.ToString(),
                Name = reader["name"]?.ToString(),
                Dob = reader["dob"] == DBNull.Value ? default : Convert.ToDateTime(reader["dob"]),
                Gender = reader["gender"]?.ToString(),
                Address = reader["address"]?.ToString(),
                Phone = reader["phone"]?.ToString(),
                Email = reader["email"]?.ToString(),
                IdCard = reader["id_card"]?.ToString(),
                BloodType = reader["blood_type"]?.ToString(),
                Allergies = reader["allergies"]?.ToString(),
                EmergencyContactName = reader["emergency_contact_name"]?.ToString(),
                EmergencyContactPhone = reader["emergency_contact_phone"]?.ToString()
            };
        }
    }
}
