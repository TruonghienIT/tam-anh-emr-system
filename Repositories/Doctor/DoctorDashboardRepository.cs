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
                string getDoctorSql = @"
                    SELECT id 
                    FROM doctors 
                    WHERE user_id = @userId";

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

                string query = "SELECT COUNT(*) FROM diseases";

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
    }
}