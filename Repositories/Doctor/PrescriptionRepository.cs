using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Helper;
using TamAnh_EMR_System.Model.Doctor; 

namespace TamAnh_EMR_System.Repositories
{
    public class PrescriptionRepository : RepositoryBase
    {
        // =========================================================
        // MAP USER -> DOCTOR
        // =========================================================
        private async Task<string> GetDoctorIdByUserIdAsync(string userId)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string sql = "SELECT id FROM doctors WHERE user_id = @userId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            var result = await cmd.ExecuteScalarAsync();

            if (result == null)
                throw new Exception("User này chưa được gán bác sĩ (doctors)");

            return result.ToString();
        }
        // 1. Lấy danh sách Toa thuốc (Cột bên trái)
        public async Task<List<Prescription>> GetAllPrescriptionsAsync()
        {
            var list = new List<Prescription>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string doctorId = await GetDoctorIdByUserIdAsync(UserSession.CurrentUser.Id);

                // JOIN medical_records, patients và doctors
                string query = @"
                SELECT 
                    m.id AS RecordId,
                    p.id AS PatientId,
                    p.name AS PatientName, 
                    d.full_name AS DoctorName,
                    m.created_at
                FROM medical_records m
                JOIN patients p ON m.patient_id = p.id
                JOIN doctors d ON m.doctor_id = d.id
                WHERE m.doctor_id = @doctorId
                ORDER BY m.created_at DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctorId", doctorId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new Prescription
                            {
                                RecordId = reader["RecordId"].ToString(),
                                PatientId = reader["PatientId"].ToString(),
                                PatientName = reader["PatientName"].ToString(),
                                DoctorName = reader["DoctorName"].ToString(),
                                Date = (DateTime)reader["created_at"],
                                Status = "Đã nhận" // Bạn có thể thêm cột Status vào bảng medical_records nếu cần
                            });
                        }
                    }
                }
            }
            return list;
        }

        // 2. Lấy chi tiết các loại thuốc của 1 toa thuốc (Cột bên phải)
        public async Task<List<MedicineItem>> GetMedicineDetailsAsync(string recordId)
        {
            var list = new List<MedicineItem>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                // JOIN prescription_details và medicines
                string query = @"
                SELECT 
                    med.name AS MedicineName,
                    pd.dosage,
                    med.instruction,
                    pd.quantity,
                    pd.frequency
                FROM prescription_details pd
                JOIN medicines med ON pd.medicine_id = med.id
                WHERE pd.record_id = @recordId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@recordId", recordId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new MedicineItem
                            {
                                Name = reader["MedicineName"].ToString(),
                                Dosage = reader["dosage"]?.ToString(),
                                Instruction = reader["instruction"]?.ToString(),
                                Frequency = reader["frequency"]?.ToString(),
                                Quantity = Convert.ToInt32(reader["quantity"])
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}