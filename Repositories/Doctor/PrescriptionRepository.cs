using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model.Doctor; // Dùng Model bạn vừa gửi

namespace TamAnh_EMR_System.Repositories
{
    public class PrescriptionRepository : RepositoryBase
    {
        // 1. Lấy danh sách Toa thuốc (Cột bên trái)
        public async Task<List<Prescription>> GetAllPrescriptionsAsync()
        {
            var list = new List<Prescription>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

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
                    ORDER BY m.created_at DESC";

                using (var command = new SqlCommand(query, connection))
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
                // JOIN prescription_details và medicines
                string query = @"
    SELECT 
        med.name AS MedicineName,
        pd.dosage,
        med.instruction,  -- Sửa pd.instruction thành med.instruction
        pd.quantity
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
                            // ... code while
                            list.Add(new MedicineItem
                            {
                                Name = reader["MedicineName"].ToString(),
                                Dosage = reader["dosage"]?.ToString(),
                                Instruction = reader["instruction"]?.ToString(), // Lấy đúng tên cột instruction
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