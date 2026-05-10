using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace TamAnh_EMR_System.Repositories
{
    public class PatientQueueDTO
    {
        public string AppointmentId { get; set; }
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string BloodType { get; set; }
    }

    public class DoctorPatientManagementRepository : RepositoryBase
    {
        // 1. Lấy danh sách chờ khám hôm nay
        public async Task<List<PatientQueueDTO>> GetPatientsQueueAsync()
        {
            var list = new List<PatientQueueDTO>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT 
                        a.id AS AppointmentId, p.id AS PatientId, p.name AS PatientName, 
                        p.gender, p.dob, p.blood_type, a.appointment_time, a.status, a.reason
                    FROM appointments a
                    JOIN patients p ON a.patient_id = p.id
                    WHERE CAST(a.appointment_date AS DATE) = CAST(GETDATE() AS DATE)
                    ORDER BY a.appointment_time ASC";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new PatientQueueDTO
                        {
                            AppointmentId = reader["AppointmentId"].ToString(),
                            PatientId = reader["PatientId"].ToString(),
                            PatientName = reader["PatientName"]?.ToString() ?? "N/A",
                            Gender = reader["gender"]?.ToString() ?? "N/A",
                            DOB = reader["dob"] != DBNull.Value ? (DateTime)reader["dob"] : DateTime.Now,
                            BloodType = reader["blood_type"]?.ToString() ?? "N/A",
                            AppointmentTime = reader["appointment_time"] != DBNull.Value ? (TimeSpan)reader["appointment_time"] : TimeSpan.Zero,
                            Status = reader["status"]?.ToString() ?? "Đang chờ",
                            Reason = reader["reason"]?.ToString() ?? "Khám tổng quát"
                        });
                    }
                }
            }
            return list;
        }

        // 2. Lưu Bệnh án mới vào CSDL
        public async Task<bool> SaveMedicalRecordAsync(string patientId, string doctorId, string diagnosis, string notes)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                // Sử dụng UUID (NEWID()) cho khóa chính
                string query = @"
                    INSERT INTO medical_records (id, patient_id, doctor_id, diagnosis, notes, created_at)
                    VALUES (NEWID(), @patientId, @doctorId, @diagnosis, @notes, GETDATE())";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@patientId", patientId);
                    // Lưu ý: Tạm thời hardcode doctorId hoặc truyền từ user đang đăng nhập
                    command.Parameters.AddWithValue("@doctorId", doctorId);
                    command.Parameters.AddWithValue("@diagnosis", diagnosis ?? "");
                    command.Parameters.AddWithValue("@notes", notes ?? "");

                    int result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }
    }
}