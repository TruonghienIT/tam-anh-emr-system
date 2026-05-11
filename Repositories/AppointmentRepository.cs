using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    /// <summary>
    /// ADO.NET implementation of IAppointmentRepository.
    /// Inherits RepositoryBase for connection string management.
    /// 
    /// Appointment ID format: LH000001, LH000002, ...
    /// Dashboard query JOINs appointments + patients + doctors for display-ready data.
    /// </summary>
    public class AppointmentRepository : RepositoryBase, IAppointmentRepository
    {
        // ================= CREATE =================
        public async Task CreateAsync(Appointment appointment, SqlConnection conn, SqlTransaction txn)
        {
            appointment.Id = await GenerateNextIdAsync(conn, txn);

            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = txn;

                cmd.CommandText = @"
                    INSERT INTO appointments 
                    (id, patient_id, doctor_id, created_by, appointment_date, 
                     appointment_time, status, reason)
                    VALUES 
                    (@id, @patient_id, @doctor_id, @created_by, @appointment_date,
                     @appointment_time, @status, @reason)";

                cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = appointment.Id;
                cmd.Parameters.Add("@patient_id", SqlDbType.VarChar, 10).Value = appointment.PatientId;
                cmd.Parameters.Add("@doctor_id", SqlDbType.VarChar, 10).Value = appointment.DoctorId;

                cmd.Parameters.Add("@appointment_date", SqlDbType.Date)
                    .Value = appointment.AppointmentDate.Date;

                TimeSpan appointmentTime;

                if (!TimeSpan.TryParse(
                        appointment.AppointmentTime?.Split('-')[0].Trim(),
                        out appointmentTime))
                {
                    appointmentTime = new TimeSpan(8, 0, 0);
                }

                cmd.Parameters.Add("@appointment_time", SqlDbType.Time)
                    .Value = appointmentTime;

                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 30)
                    .Value = "Đang chờ";

                cmd.Parameters.Add("@reason", SqlDbType.NVarChar, 500)
                    .Value = (object?)appointment.Reason ?? DBNull.Value;

                cmd.Parameters.Add("@created_by", SqlDbType.VarChar, 10)
                    .Value = (object?)appointment.CreatedBy ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ================= GET DASHBOARD APPOINTMENTS =================
        public async Task<List<DashboardAppointment>> GetDashboardAppointmentsAsync(DateTime? date = null)
        {
            var list = new List<DashboardAppointment>();
            var targetDate = date ?? DateTime.Today;

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                await conn.OpenAsync();
                cmd.Connection = conn;

                cmd.CommandText = @"
                    SELECT 
                        a.id AS appointment_id,
                        a.appointment_time,
                        a.appointment_date,
                        a.status,
                        a.reason,
                        p.id AS patient_id,
                        p.name AS patient_name,
                        p.gender AS patient_gender,
                        p.dob AS patient_dob,
                        d.id AS doctor_id,
                        d.full_name AS doctor_name,
                        d.specialization AS department
                    FROM appointments a
                    INNER JOIN patients p ON a.patient_id = p.id
                    INNER JOIN doctors d ON a.doctor_id = d.id
                    WHERE a.appointment_date = @date
                    ORDER BY a.appointment_time ASC";

                cmd.Parameters.Add("@date", SqlDbType.Date).Value = targetDate;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var patientName = reader["patient_name"]?.ToString() ?? "";
                        var patientGender = reader["patient_gender"]?.ToString() ?? "";
                        var patientDob = reader["patient_dob"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(reader["patient_dob"]);

                        int age = 0;
                        if (patientDob.HasValue)
                        {
                            age = DateTime.Today.Year - patientDob.Value.Year;
                            if (patientDob.Value.Date > DateTime.Today.AddYears(-age)) age--;
                        }

                        // Generate initials from patient name
                        string initials = GenerateInitials(patientName);

                        // Generate avatar color based on patient name hash
                        string avatarColor = GenerateAvatarColor(patientName);

                        var appointmentDate = Convert.ToDateTime(reader["appointment_date"]);
                        string dayOfWeek = appointmentDate.DayOfWeek switch
                        {
                            DayOfWeek.Monday => "THỨ 2",
                            DayOfWeek.Tuesday => "THỨ 3",
                            DayOfWeek.Wednesday => "THỨ 4",
                            DayOfWeek.Thursday => "THỨ 5",
                            DayOfWeek.Friday => "THỨ 6",
                            DayOfWeek.Saturday => "THỨ 7",
                            DayOfWeek.Sunday => "CN",
                            _ => ""
                        };

                        var timeValue = (TimeSpan)reader["appointment_time"];
                        var displayTime = timeValue.ToString(@"hh\:mm");

                        list.Add(new DashboardAppointment
                        {
                            Time = displayTime,
                            Date = $"{dayOfWeek}, {appointmentDate:dd/MM}",
                            PatientName = patientName,
                            PatientInitials = initials,
                            GenderAge = $"{patientGender}, {age} tuổi",
                            DoctorName = reader["doctor_name"]?.ToString() ?? "",
                            Department = reader["department"]?.ToString() ?? "",
                            Service = reader["reason"]?.ToString() ?? "Khám tổng quát",
                            Status = reader["status"]?.ToString() ?? "Đang chờ",
                            AvatarColor = avatarColor
                        });
                    }
                }
            }

            return list;
        }

        // ================= CHECK DOCTOR SCHEDULE CONFLICT =================
        public async Task<bool> CheckDoctorScheduleConflictAsync(string doctorId, DateTime date, string timeSlot)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                await conn.OpenAsync();
                cmd.Connection = conn;

                cmd.CommandText = @"
                    SELECT COUNT(*) FROM appointments
                    WHERE doctor_id = @doctor_id 
                      AND appointment_date = @date 
                      AND appointment_time = @time_slot
                      AND status NOT IN (N'Đã hủy')";

                cmd.Parameters.Add("@doctor_id", SqlDbType.VarChar, 10).Value = doctorId;
                cmd.Parameters.Add("@date", SqlDbType.Date).Value = date;
                cmd.Parameters.Add("@time_slot", SqlDbType.VarChar, 20).Value = timeSlot;

                var count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        // ================= GENERATE NEXT ID =================
        public async Task<string> GenerateNextIdAsync(SqlConnection conn, SqlTransaction txn)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = txn;

                cmd.CommandText = @"
            SELECT ISNULL(MAX(CAST(SUBSTRING(id, 3, 6) AS INT)), 0) + 1
            FROM appointments";

                var result = await cmd.ExecuteScalarAsync();

                int nextNum = Convert.ToInt32(result);

                return $"LH{nextNum:D6}";
            }
        }

        // ================= GET TODAY STATISTICS =================
        public async Task<Dictionary<string, int>> GetTodayStatisticsAsync()
        {
            var stats = new Dictionary<string, int>
            {
                { "total", 0 },
                { "waiting", 0 },
                { "completed", 0 },
                { "cancelled", 0 },
                { "in_progress", 0 }
            };

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                await conn.OpenAsync();
                cmd.Connection = conn;

                cmd.CommandText = @"
                    SELECT 
                        COUNT(*) AS total,
                        SUM(CASE WHEN status = N'Đang chờ' THEN 1 ELSE 0 END) AS waiting,
                        SUM(CASE WHEN status = N'Hoàn thành' THEN 1 ELSE 0 END) AS completed,
                        SUM(CASE WHEN status = N'Đã hủy' THEN 1 ELSE 0 END) AS cancelled,
                        SUM(CASE WHEN status = N'Đang khám' THEN 1 ELSE 0 END) AS in_progress
                    FROM appointments
                    WHERE appointment_date = @today";

                cmd.Parameters.Add("@today", SqlDbType.Date).Value = DateTime.Today;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        stats["total"] = reader["total"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total"]);
                        stats["waiting"] = reader["waiting"] == DBNull.Value ? 0 : Convert.ToInt32(reader["waiting"]);
                        stats["completed"] = reader["completed"] == DBNull.Value ? 0 : Convert.ToInt32(reader["completed"]);
                        stats["cancelled"] = reader["cancelled"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cancelled"]);
                        stats["in_progress"] = reader["in_progress"] == DBNull.Value ? 0 : Convert.ToInt32(reader["in_progress"]);
                    }
                }
            }

            return stats;
        }
        public async Task<string> GenerateNextIdAsync()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
            SELECT ISNULL(MAX(CAST(SUBSTRING(id, 3, 6) AS INT)), 0) + 1
            FROM appointments";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    int nextNum = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return $"LH{nextNum:D6}";
                }
            }
        }
        public async Task AddAsync(Appointment appointment)
        {
            using (SqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();

                string query = @"
        INSERT INTO appointments
        (
            id,
            patient_id,
            doctor_id,
            appointment_date,
            appointment_time,
            status,
            reason,
            created_by
        )
        VALUES
        (
            @id,
            @patient_id,
            @doctor_id,
            @appointment_date,
            @appointment_time,
            @status,
            @reason,
            @created_by
        )";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10)
                        .Value = appointment.Id;

                    cmd.Parameters.Add("@patient_id", SqlDbType.VarChar, 10)
                        .Value = appointment.PatientId;

                    cmd.Parameters.Add("@doctor_id", SqlDbType.VarChar, 10)
                        .Value = appointment.DoctorId;

                    cmd.Parameters.Add("@appointment_date", SqlDbType.Date)
                        .Value = appointment.AppointmentDate.Date;

                    TimeSpan appointmentTime;

                    if (!TimeSpan.TryParse(
                            appointment.AppointmentTime?.Split('-')[0].Trim(),
                            out appointmentTime))
                    {
                        appointmentTime = new TimeSpan(8, 0, 0);
                    }

                    cmd.Parameters.Add("@appointment_time", SqlDbType.Time)
                        .Value = appointmentTime;

                    cmd.Parameters.Add("@status", SqlDbType.NVarChar, 30)
                        .Value = "Đang chờ";

                    cmd.Parameters.Add("@reason", SqlDbType.NVarChar, 500)
                        .Value = (object?)appointment.Reason ?? DBNull.Value;

                    cmd.Parameters.Add("@created_by", SqlDbType.VarChar, 10)
                        .Value = DBNull.Value;

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // ================= HELPER: Generate Initials =================
        private string GenerateInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "??";

            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            // First char of first name + first char of last name
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        // ================= HELPER: Generate Avatar Color =================
        private string GenerateAvatarColor(string name)
        {
            string[] colors = { "#6366F1", "#EC4899", "#10B981", "#F59E0B", "#3B82F6", "#8B5CF6", "#EF4444", "#06B6D4" };
            if (string.IsNullOrWhiteSpace(name)) return colors[0];

            int hash = 0;
            foreach (char c in name) hash += c;
            return colors[hash % colors.Length];
        }
        public async Task<List<ChartDataPoint>> GetTodayChartDataAsync()
        {
            var result = new List<ChartDataPoint>();

            using (var conn = GetConnection())
            {
                await conn.OpenAsync();

                string query = @"
        SELECT 
            FORMAT(appointment_time, 'hh\:mm') AS HourLabel,
            COUNT(*) AS Total
        FROM appointments
        WHERE appointment_date = CAST(GETDATE() AS DATE)
        GROUP BY appointment_time
        ORDER BY appointment_time";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new ChartDataPoint
                        {
                            Label = reader["HourLabel"].ToString(),
                            Value = Convert.ToInt32(reader["Total"])
                        });
                    }
                }
            }

            return result;
        }
    }
}
