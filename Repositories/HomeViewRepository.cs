using System;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    public class HomeViewRepository : RepositoryBase
    {
        public int GetPatientCount()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM patients", conn);
            return (int)cmd.ExecuteScalar();
        }

        public int GetAppointmentCount()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM appointments", conn);
            return (int)cmd.ExecuteScalar();
        }

        public int GetMedicalRecordCount()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM medical_records", conn);
            return (int)cmd.ExecuteScalar();
        }

        public int GetDoctorCount()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM doctors", conn);
            return (int)cmd.ExecuteScalar();
        }

        public int GetPendingAppointmentCount()
        {
            using var conn = GetConnection();

            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM appointments
                WHERE status = N'Đang chờ'
            ", conn);

            return (int)cmd.ExecuteScalar();
        }

        public int GetTodayMedicalRecordCount()
        {
            using var conn = GetConnection();

            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM medical_records
                WHERE CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)
            ", conn);

            return (int)cmd.ExecuteScalar();
        }

        public int GetSpecializationCount()
        {
            using var conn = GetConnection();

            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT COUNT(DISTINCT specialization)
                FROM doctors
                WHERE specialization IS NOT NULL
            ", conn);

            return (int)cmd.ExecuteScalar();
        }

        public ObservableCollection<Appointment> GetTodayAppointments()
        {
            var list = new ObservableCollection<Appointment>();

            using var conn = GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT 
                    a.id,
                    a.appointment_date,
                    a.appointment_time,
                    a.status,
                    a.reason,
                    p.name AS patient_name,
                    d.full_name AS doctor_name
                FROM appointments a
                INNER JOIN patients p ON a.patient_id = p.id
                INNER JOIN doctors d ON a.doctor_id = d.id
                WHERE CAST(a.appointment_date AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY a.appointment_time
            ", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Appointment
                {
                    Id = reader["id"].ToString(),
                    AppointmentDate = Convert.ToDateTime(reader["appointment_date"]),
                    AppointmentTime = reader["appointment_time"].ToString(),
                    Status = reader["status"]?.ToString(),
                    Reason = reader["reason"]?.ToString(),

                    Patient = new Patients
                    {
                        Name = reader["patient_name"].ToString()
                    },

                    Doctor = new Doctors
                    {
                        FullName = reader["doctor_name"].ToString()
                    }
                });
            }

            return list;
        }

        public string GetPatientGrowth()
        {
            using var conn = GetConnection();

            conn.Open();

            using var cmd = new SqlCommand(@"
                DECLARE @CurrentMonth INT;
                DECLARE @LastMonth INT;

                SELECT @CurrentMonth = COUNT(*)
                FROM patients
                WHERE MONTH(created_at) = MONTH(GETDATE())
                  AND YEAR(created_at) = YEAR(GETDATE());

                SELECT @LastMonth = COUNT(*)
                FROM patients
                WHERE MONTH(created_at) = MONTH(DATEADD(MONTH, -1, GETDATE()))
                  AND YEAR(created_at) = YEAR(DATEADD(MONTH, -1, GETDATE()));

                SELECT @CurrentMonth AS CurrentMonth,
                       @LastMonth AS LastMonth
            ", conn);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int current = Convert.ToInt32(reader["CurrentMonth"]);
                int last = Convert.ToInt32(reader["LastMonth"]);

                if (last == 0)
                {
                    return current > 0
                        ? "+100% tháng này"
                        : "0% tháng này";
                }

                double percent = ((double)(current - last) / last) * 100;

                string sign = percent >= 0 ? "+" : "";

                return $"{sign}{Math.Round(percent)}% tháng này";
            }

            return "0% tháng này";
        }

        public ObservableCollection<ChartDataPoint> GetPatientChartByGender()
        {
            var data = new ObservableCollection<ChartDataPoint>();

            using var conn = GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT GenderType, COUNT(p.gender) AS Total
                FROM
                (
                    VALUES
                    (N'Nam'),
                    (N'Nữ'),
                    (N'Khác')
                ) AS G(GenderType)

                LEFT JOIN patients p
                    ON p.gender = G.GenderType

                GROUP BY GenderType
            ", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                data.Add(new ChartDataPoint
                {
                    Label = reader["GenderType"].ToString(),
                    Value = Convert.ToDouble(reader["Total"])
                });
            }

            return data;
        }

        public ObservableCollection<ChartDataPoint> GetPatientChartByAge()
        {
            var data = new ObservableCollection<ChartDataPoint>();

            using var conn = GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                WITH PatientAge AS
                (
                    SELECT
                        CASE
                            WHEN DATEDIFF(YEAR, dob, GETDATE()) < 18 THEN N'<18'
                            WHEN DATEDIFF(YEAR, dob, GETDATE()) BETWEEN 18 AND 40 THEN N'18-40'
                            WHEN DATEDIFF(YEAR, dob, GETDATE()) BETWEEN 41 AND 60 THEN N'41-60'
                            ELSE N'>60'
                        END AS AgeGroup
                    FROM patients
                )

                SELECT AgeType, COUNT(P.AgeGroup) AS Total
                FROM
                (
                    VALUES
                    (N'<18'),
                    (N'18-40'),
                    (N'41-60'),
                    (N'>60')
                ) AS A(AgeType)

                LEFT JOIN PatientAge P
                    ON P.AgeGroup = A.AgeType

                GROUP BY AgeType
            ", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                data.Add(new ChartDataPoint
                {
                    Label = reader["AgeType"].ToString(),
                    Value = Convert.ToDouble(reader["Total"])
                });
            }

            return data;
        }

        public ObservableCollection<ChartDataPoint> GetPatientChartByBloodType()
        {
            var data = new ObservableCollection<ChartDataPoint>();

            using var conn = GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT BloodType, COUNT(p.blood_type) AS Total
                FROM
                (
                    VALUES
                    ('A'),
                    ('B'),
                    ('AB'),
                    ('O')
                ) AS B(BloodType)

                LEFT JOIN patients p
                    ON p.blood_type = B.BloodType

                GROUP BY BloodType
            ", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                data.Add(new ChartDataPoint
                {
                    Label = reader["BloodType"].ToString(),
                    Value = Convert.ToDouble(reader["Total"])
                });
            }

            return data;
        }

        public ObservableCollection<NotificationItem> GetRecentActivities()
        {
            var list = new ObservableCollection<NotificationItem>();

            using var conn = GetConnection();

            conn.Open();

            string query = @"
                SELECT TOP 10 *
                FROM
                (
                    -- =========================================
                    -- THÊM BỆNH NHÂN MỚI
                    -- =========================================
                    SELECT
                        N'Thêm bệnh nhân mới' AS Title,

                        CONCAT
                        (
                            N'Bệnh nhân: ',
                            p.name
                        ) AS Description,

                        p.created_at AS CreatedAt,

                        'success' AS Type

                    FROM patients p

                    UNION ALL

                    -- =========================================
                    -- ĐẶT LỊCH KHÁM
                    -- =========================================
                    SELECT
                        N'Đặt lịch khám' AS Title,

                        CONCAT
                        (
                            N'Bệnh nhân: ',
                            p.name,
                            N' - ',
                            ISNULL(a.status, N'Đang chờ')
                        ) AS Description,

                        DATEADD
                        (
                            SECOND,
                            DATEDIFF
                            (
                                SECOND,
                                '00:00:00',
                                a.appointment_time
                            ),
                            CAST(a.appointment_date AS DATETIME)
                        ) AS CreatedAt,

                        CASE
                            WHEN a.status = N'Đã hủy'
                                THEN 'error'

                            WHEN a.status = N'Hoàn thành'
                                THEN 'success'

                            WHEN a.status = N'Đang khám'
                                THEN 'warning'

                            ELSE 'info'
                        END AS Type

                    FROM appointments a

                    INNER JOIN patients p
                        ON a.patient_id = p.id

                    WHERE
                        DATEADD
                        (
                            SECOND,
                            DATEDIFF
                            (
                                SECOND,
                                '00:00:00',
                                a.appointment_time
                            ),
                            CAST(a.appointment_date AS DATETIME)
                        ) <= GETDATE()

                    UNION ALL

                    -- =========================================
                    -- TẠO BỆNH ÁN
                    -- =========================================
                    SELECT
                        N'Tạo bệnh án mới' AS Title,

                        CONCAT
                        (
                            N'Bệnh nhân: ',
                            p.name
                        ) AS Description,

                        mr.created_at AS CreatedAt,

                        'info' AS Type

                    FROM medical_records mr

                    INNER JOIN patients p
                        ON mr.patient_id = p.id

                ) AS Activities

                WHERE CreatedAt IS NOT NULL

                ORDER BY
                    ABS(DATEDIFF(SECOND, CreatedAt, GETDATE()))
            ";

            using var cmd = new SqlCommand(query, conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                DateTime createdAt =
                    reader["CreatedAt"] == DBNull.Value
                        ? DateTime.Now
                        : Convert.ToDateTime(reader["CreatedAt"]);

                list.Add(new NotificationItem
                {
                    Title = reader["Title"]?.ToString() ?? "",

                    Description = reader["Description"]?.ToString() ?? "",

                    CreatedAt = createdAt,

                    TimeText = GetTimeAgo(createdAt),

                    Type = reader["Type"]?.ToString() ?? "info"
                });
            }

            return list;
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            TimeSpan span = DateTime.Now - dateTime;

            if (span.TotalSeconds < 60)
                return "Vừa xong";

            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} phút trước";

            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} giờ trước";

            if (span.TotalDays < 30)
                return $"{(int)span.TotalDays} ngày trước";

            return dateTime.ToString("dd/MM/yyyy");
        }

        #region Today Progress
        public double GetCompletedAppointmentProgress()
        {
            using var conn = GetConnection();

            conn.Open();

            using var cmd = new SqlCommand(@"
                DECLARE @Total INT =
                (
                    SELECT COUNT(*)
                    FROM appointments
                    WHERE CAST(appointment_date AS DATE) = CAST(GETDATE() AS DATE)
                );

                DECLARE @Completed INT =
                (
                    SELECT COUNT(*)
                    FROM appointments
                    WHERE CAST(appointment_date AS DATE) = CAST(GETDATE() AS DATE)
                    AND status = N'Hoàn thành'
                );

                SELECT
                    CASE
                        WHEN @Total = 0 THEN 0
                        ELSE (@Completed * 100.0 / @Total)
                    END
            ", conn);

            double value = Convert.ToDouble(cmd.ExecuteScalar());

            return Math.Min(value, 100);
        }

        public double GetMedicalRecordProgress()
        {
            using var conn = GetConnection();

            conn.Open();

            using var cmd = new SqlCommand(@"
                DECLARE @PatientToday INT =
                (
                    SELECT COUNT(*)
                    FROM appointments
                    WHERE CAST(appointment_date AS DATE) = CAST(GETDATE() AS DATE)
                );

                DECLARE @RecordToday INT =
                (
                    SELECT COUNT(*)
                    FROM medical_records
                    WHERE CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)
                );

                SELECT
                    CASE
                        WHEN @PatientToday = 0 THEN 0
                        ELSE (@RecordToday * 100.0 / @PatientToday)
                    END
            ", conn);

            double value = Convert.ToDouble(cmd.ExecuteScalar());

            return Math.Min(value, 100);
        }

        public double GetPatientReceptionProgress()
        {
            using var conn = GetConnection();

            conn.Open();

            using var cmd = new SqlCommand(@"
                DECLARE @Total INT =
                (
                    SELECT COUNT(*)
                    FROM appointments
                    WHERE CAST(appointment_date AS DATE) = CAST(GETDATE() AS DATE)
                );

                DECLARE @Handled INT =
                (
                    SELECT COUNT(*)
                    FROM appointments
                    WHERE CAST(appointment_date AS DATE) = CAST(GETDATE() AS DATE)
                    AND status IN (N'Đang khám', N'Hoàn thành')
                );

                SELECT
                    CASE
                        WHEN @Total = 0 THEN 0
                        ELSE (@Handled * 100.0 / @Total)
                    END
            ", conn);

            double value = Convert.ToDouble(cmd.ExecuteScalar());

            return Math.Min(value, 100);
        }
        #endregion
    }
}