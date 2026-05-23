using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Helper;

namespace TamAnh_EMR_System.Repositories
{
    // Class DTO (Data Transfer Object) dùng để hứng dữ liệu từ câu lệnh JOIN 3 bảng
    public class AppointmentDTO
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; }
    }

    public class DoctorDashboardRepository : RepositoryBase
    {
        /// <summary>
        /// Lấy danh sách lịch hẹn của ngày hôm nay, kết nối với bảng patients và doctors để lấy tên
        /// </summary>
        public async Task<List<AppointmentDTO>> GetTodaysAppointmentsAsync()
        {
            var list = new List<AppointmentDTO>();
            string doctorId;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string getDoctorSql = @"SELECT id FROM doctors WHERE user_id = @userId";

                using (var cmdDoctor = new SqlCommand(getDoctorSql, connection))
                {
                    if (UserSession.CurrentUser == null)
                    {
                        return new List<AppointmentDTO>();
                    }

                    cmdDoctor.Parameters.AddWithValue("@userId", UserSession.CurrentUser.Id);

                    var result = await cmdDoctor.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                    {
                        return new List<AppointmentDTO>();
                    }

                    doctorId = result.ToString();
                }

                // Câu lệnh SQL: Lọc theo ngày hiện tại và sắp xếp theo giờ hẹn
                string query = @"
                    SELECT 
                        p.name AS PatientName,
                        d.full_name AS DoctorName,
                        a.appointment_time AS AppointmentTime,
                        a.status AS Status
                    FROM appointments a
                    LEFT JOIN patients p ON a.patient_id = p.id
                    LEFT JOIN doctors d ON a.doctor_id = d.id
                    WHERE CAST(a.appointment_date AS DATE) = CAST(GETDATE() AS DATE) AND a.doctor_id = @doctorId
                    ORDER BY a.appointment_time ASC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctorId", doctorId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new AppointmentDTO
                            {
                                // Kiểm tra DBNull để tránh lỗi văng app nếu dữ liệu trong SQL bị rỗng
                                PatientName = reader["PatientName"] != DBNull.Value ? reader["PatientName"].ToString() : "Khách vãng lai",
                                DoctorName = reader["DoctorName"] != DBNull.Value ? reader["DoctorName"].ToString() : "Chưa xếp bác sĩ",
                                AppointmentTime = reader["AppointmentTime"] != DBNull.Value ? (TimeSpan)reader["AppointmentTime"] : TimeSpan.Zero,
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Đang chờ"
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Đếm tổng số lượng bệnh (dùng cho phần thống kê)
        /// </summary>
        public async Task<int> GetTotalDiseasesCountAsync()
        {
            int count = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                // Đếm tổng số hồ sơ bệnh án
                string query = @"SELECT COUNT(*) FROM medical_records";

                using (var command = new SqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();

                    if (result != DBNull.Value)
                    {
                        count = Convert.ToInt32(result);
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Lấy số lượng appointments theo giờ trong ngày hôm nay
        /// </summary>
        public async Task<Dictionary<string, int>> GetAppointmentsByTimeSlotAsync()
        {
            var result = new Dictionary<string, int>();
            string doctorId;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string getDoctorSql = @"
                    SELECT id 
                    FROM doctors 
                    WHERE user_id = @userId";

                using (var cmdDoctor = new SqlCommand(getDoctorSql, connection))
                {
                    if (UserSession.CurrentUser == null)
                    {
                        return new Dictionary<string, int>();
                    }

                    cmdDoctor.Parameters.AddWithValue("@userId", UserSession.CurrentUser.Id);
                    var docId = await cmdDoctor.ExecuteScalarAsync();

                    if (docId == null || docId == DBNull.Value)
                    {
                        return new Dictionary<string, int>();
                    }

                    doctorId = docId.ToString();
                }

                // Lấy số lượng appointments theo giờ
                string query = @"
                    SELECT 
                        DATEPART(HOUR, CAST(a.appointment_time AS TIME)) AS Hour,
                        COUNT(*) AS Count
                    FROM appointments a
                    WHERE CAST(a.appointment_date AS DATE) = CAST(GETDATE() AS DATE) 
                    AND a.doctor_id = @doctorId
                    GROUP BY DATEPART(HOUR, CAST(a.appointment_time AS TIME))
                    ORDER BY Hour";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctorId", doctorId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int hour = Convert.ToInt32(reader["Hour"]);
                            int count = Convert.ToInt32(reader["Count"]);
                            string timeSlot = $"{hour:D2}:00";
                            result[timeSlot] = count;
                        }
                    }
                }
            }

            // Điền các giờ không có data bằng 0 (từ 7h đến 17h)
            for (int h = 7; h < 18; h++)
            {
                string slot = $"{h:D2}:00";
                if (!result.ContainsKey(slot))
                {
                    result[slot] = 0;
                }
            }

            return result;
        }
        /// <summary>
        /// Thống kê số lượng chẩn đoán gom nhóm theo tên bệnh (dành cho Bar Chart)
        /// </summary>
        public async Task<Dictionary<string, int>> GetDiseaseStatisticsAsync()
        {
            var result = new Dictionary<string, int>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT TOP 10
                        d.disease_name AS DiseaseName,
                        COUNT(mr.id) AS Count
                    FROM medical_records mr
                    INNER JOIN diseases d 
                        ON mr.icd_code = d.icd_code
                    GROUP BY d.disease_name
                    ORDER BY Count DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string diseaseName = reader["DiseaseName"] != DBNull.Value ? reader["DiseaseName"].ToString() : "Chưa xác định";
                            int count = Convert.ToInt32(reader["Count"]);
                            result[diseaseName] = count;
                        }
                    }
                }
            }
            return result;
        }
        public async Task<List<int>> GetPatientTrendLast7DaysAsync()
        {
            // Tạo mảng 7 phần tử (mặc định = 0) đại diện cho 7 ngày qua (vị trí 6 là hôm nay)
            var result = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
            string doctorId;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                // 1. Lấy doctorId (Bạn có thể tái sử dụng đoạn code lấy doctorId ở các hàm trước)
                string getDoctorSql = "SELECT id FROM doctors WHERE user_id = @userId";
                using (var cmdDoctor = new SqlCommand(getDoctorSql, connection))
                {
                    if (UserSession.CurrentUser == null) return result;
                    cmdDoctor.Parameters.AddWithValue("@userId", UserSession.CurrentUser.Id);
                    var docId = await cmdDoctor.ExecuteScalarAsync();
                    if (docId == null || docId == DBNull.Value) return result;
                    doctorId = docId.ToString();
                }

                // 2. Truy vấn số lượng lịch hẹn 7 ngày qua
                string query = @"
            SELECT 
                CAST(appointment_date AS DATE) AS ApptDate,
                COUNT(*) AS PatientCount
            FROM appointments
            WHERE doctor_id = @doctorId
              AND appointment_date >= CAST(DATEADD(day, -6, GETDATE()) AS DATE)
              AND appointment_date <= CAST(GETDATE() AS DATE)
            GROUP BY CAST(appointment_date AS DATE)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctorId", doctorId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var today = DateTime.Today;
                        while (await reader.ReadAsync())
                        {
                            DateTime apptDate = Convert.ToDateTime(reader["ApptDate"]);
                            int count = Convert.ToInt32(reader["PatientCount"]);

                            // Tính khoảng cách ngày so với hôm nay (0 đến 6)
                            int daysAgo = (today - apptDate).Days;
                            if (daysAgo >= 0 && daysAgo <= 6)
                            {
                                // daysAgo = 0 (hôm nay) -> index = 6
                                // daysAgo = 6 (6 ngày trước) -> index = 0
                                result[6 - daysAgo] = count;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }

}