using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    public class PatientQueueDTO
    {
        public string AppointmentId { get; set; }
        public string PatientId { get; set; }

        public string DoctorId { get; set; }
        public string PatientName { get; set; }
        public string PhoneNumber { get; set; } 
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string BloodType { get; set; }
    }

    public class DoctorPatientManagementRepository : RepositoryBase
    {

        public async Task<List<Diseases>> GetDiseasesAsync()
        {
            var list = new List<Diseases>();

            using (var conn = GetConnection())
            {
                await conn.OpenAsync();

                string sql = "SELECT icd_code, disease_name, description FROM diseases";

                using (var cmd = new SqlCommand(sql, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new Diseases
                        {
                            IcdCode = r["icd_code"].ToString(),
                            DiseaseName = r["disease_name"].ToString(),
                            Description = r["description"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }
        // =========================================================
        // 1. LẤY DANH SÁCH BỆNH NHÂN CHỜ KHÁM HÔM NAY
        // =========================================================
        public async Task<List<PatientQueueDTO>> GetPatientsQueueAsync()
        {
            var list = new List<PatientQueueDTO>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        a.id AS AppointmentId,
                        a.doctor_id AS DoctorId,
                        p.id AS PatientId,
                        p.name AS PatientName,
                        p.phone AS PhoneNumber,
                        p.gender,
                        p.dob,
                        p.blood_type,
                        a.appointment_time,
                        a.status,
                        a.reason
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
                            AppointmentId = reader["AppointmentId"]?.ToString(),
                            DoctorId = reader["DoctorId"]?.ToString(),
                            PatientId = reader["PatientId"]?.ToString(),
                            PatientName = reader["PatientName"]?.ToString() ?? "N/A",
                            PhoneNumber = reader["PhoneNumber"]?.ToString() ?? "",
                            Gender = reader["gender"]?.ToString() ?? "N/A",
                            DOB = reader["dob"] != DBNull.Value
                                ? (DateTime)reader["dob"]
                                : DateTime.Now,
                            BloodType = reader["blood_type"]?.ToString() ?? "N/A",
                            AppointmentTime = reader["appointment_time"] != DBNull.Value
                                ? (TimeSpan)reader["appointment_time"]
                                : TimeSpan.Zero,
                            Status = reader["status"]?.ToString() ?? "Đang chờ",
                            Reason = reader["reason"]?.ToString() ?? "Khám tổng quát"
                        });
                    }
                }
            }

            return list;
        }

        // =========================================================
        // 2. LƯU TOÀN BỘ BỆNH ÁN
        // =========================================================
        public async Task<bool> SaveMedicalRecordAsync(
            string patientId,
            string appointmentId,
            string doctorId,
            string icdCode,
            string diagnosis,
            string treatment,
            string notes,
            string pulse,
            string bloodPressure,
            string temperature,
            string spo2,
            string labTestName,
            string labResult)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    string medicalRecordId = await GenerateIdAsync(connection, transaction, "medical_records", "MR");
                    string labResultId = await GenerateIdAsync(connection, transaction, "lab_results", "LR");


                    // =====================================================
                    // 1. INSERT MEDICAL RECORD (FULL VITAL SIGNS)
                    // =====================================================
                    string insertMedicalRecord = @"
                INSERT INTO medical_records
                (
                    id,
                    patient_id,
                    doctor_id,
                    icd_code,
                    diagnosis,
                    treatment,
                    notes,
                    pulse,
                    blood_pressure,
                    temperature,
                    spo2,
                    created_at
                )
                VALUES
                (
                    @id,
                    @patientId,
                    @doctorId,
                    @icdCode,
                    @diagnosis,
                    @treatment,
                    @notes,
                    @pulse,
                    @bloodPressure,
                    @temperature,
                    @spo2,
                    GETDATE()
                )";

                    using (var cmd = new SqlCommand(insertMedicalRecord, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@id", medicalRecordId);
                        cmd.Parameters.AddWithValue("@patientId", patientId);
                        cmd.Parameters.AddWithValue("@doctorId", doctorId);
                        cmd.Parameters.AddWithValue("@icdCode",
                            string.IsNullOrWhiteSpace(icdCode) ? DBNull.Value : icdCode);

                        cmd.Parameters.AddWithValue("@diagnosis", diagnosis ?? "");
                        cmd.Parameters.AddWithValue("@treatment", treatment ?? "");
                        cmd.Parameters.AddWithValue("@notes", notes ?? "");

                        cmd.Parameters.AddWithValue("@pulse",
                            int.TryParse(pulse, out int p) ? p : DBNull.Value);

                        cmd.Parameters.AddWithValue("@bloodPressure",
                            string.IsNullOrWhiteSpace(bloodPressure) ? DBNull.Value : bloodPressure);

                        cmd.Parameters.AddWithValue("@temperature",
                            decimal.TryParse(temperature, out decimal t) ? t : DBNull.Value);

                        cmd.Parameters.AddWithValue("@spo2",
                            int.TryParse(spo2, out int s) ? s : DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // =====================================================
                    // 2. LAB RESULT
                    // =====================================================
                    if (!string.IsNullOrWhiteSpace(labTestName))
                    {
                        string insertLab = @"
                    INSERT INTO lab_results
                    (
                        id,
                        record_id,
                        test_name,
                        result,
                        test_date
                    )
                    VALUES
                    (
                        @id,
                        @recordId,
                        @testName,
                        @result,
                        GETDATE()
                    )";

                        using (var cmd = new SqlCommand(insertLab, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", labResultId);
                            cmd.Parameters.AddWithValue("@recordId", medicalRecordId);
                            cmd.Parameters.AddWithValue("@testName", labTestName);
                            cmd.Parameters.AddWithValue("@result", labResult ?? "");

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // =====================================================
                    // 3. UPDATE APPOINTMENT (CHỈ STATUS)
                    // =====================================================
                    string updateAppointment = @"
                UPDATE appointments
                SET status = N'Hoàn thành'
                WHERE id = @appointmentId";

                    using (var cmd = new SqlCommand(updateAppointment, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@appointmentId", appointmentId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // =====================================================
                    // COMMIT
                    // =====================================================
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    MessageBox.Show(
                        $"Lỗi khi lưu bệnh án:\n{ex.Message}",
                        "Database Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return false;
                }
            }
        }

        public async Task<bool> UpdatePatientStatusAsync(string appointmentId, string newStatus)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
                        UPDATE appointments
                        SET status = @status
                        WHERE id = @appointmentId
                    ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@status", newStatus);
                        command.Parameters.AddWithValue("@appointmentId", appointmentId);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(
                    $"Lỗi SQL khi cập nhật trạng thái:\n{sqlEx.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi hệ thống:\n{ex.Message}",
                    "System Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return false;
            }
        }

        // =========================================================
        // LẤY TẤT CẢ HỒ SƠ BỆNH ÁN (MEDICAL RECORDS)
        // =========================================================
        public async Task<List<MedicalRecords>> GetAllMedicalRecordsAsync()
        {
            var list = new List<MedicalRecords>();

            using (var conn = GetConnection())
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT
                        mr.id,
                        mr.patient_id,
                        mr.doctor_id,
                        mr.icd_code,
                        mr.diagnosis,
                        mr.treatment,
                        mr.notes,
                        mr.created_at,
                        mr.pulse,
                        mr.blood_pressure,
                        mr.temperature,
                        mr.spo2,
                        p.name AS patient_name,
                        d.full_name AS doctor_name,
                        ds.disease_name
                    FROM medical_records mr
                    LEFT JOIN patients p ON mr.patient_id = p.id
                    LEFT JOIN doctors d ON mr.doctor_id = d.id
                    LEFT JOIN diseases ds ON mr.icd_code = ds.icd_code
                    ORDER BY mr.created_at DESC";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new MedicalRecords
                        {
                            Id = reader["id"]?.ToString() ?? "",
                            PatientId = reader["patient_id"]?.ToString() ?? "",
                            DoctorId = reader["doctor_id"]?.ToString() ?? "",
                            IcdCode = reader["icd_code"]?.ToString() ?? "",
                            Diagnosis = reader["diagnosis"]?.ToString() ?? "",
                            Treatment = reader["treatment"]?.ToString() ?? "",
                            Notes = reader["notes"]?.ToString() ?? "",
                            Pulse = reader["pulse"]?.ToString() ?? "",
                            BloodPressure = reader["blood_pressure"]?.ToString() ?? "",
                            Temperature = reader["temperature"]?.ToString() ?? "",
                            SPO2 = reader["spo2"]?.ToString() ?? "",
                            CreatedAt = reader["created_at"] != DBNull.Value ? (DateTime)reader["created_at"] : DateTime.MinValue,
                            Patient = new Patients { Name = reader["patient_name"]?.ToString() ?? "N/A" },
                            Doctor = new Doctors { FullName = reader["doctor_name"]?.ToString() ?? "N/A" },
                            Disease = new Diseases { DiseaseName = reader["disease_name"]?.ToString() ?? "N/A" }
                        });
                    }
                }
            }

            return list;
        }

        private async Task<string> GenerateIdAsync(SqlConnection connection, SqlTransaction transaction, string table, string prefix)
        {
            string query = $@"
                SELECT ISNULL(MAX(CAST(SUBSTRING(id, LEN(@prefix) + 1, LEN(id)) AS INT)), 0)
                FROM {table}
                WHERE id LIKE @prefix + '%'";

            using (var cmd = new SqlCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@prefix", prefix);

                int max = (int)await cmd.ExecuteScalarAsync();
                int next = max + 1;

                return prefix + next.ToString("D3");
            }
        }

        // Hàm Xóa bệnh án
        public async Task<bool> DeleteMedicalRecordAsync(string recordId)
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                string query = "DELETE FROM medical_records WHERE id = @id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", recordId);
                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }

        // Hàm Cập nhật bệnh án
        public async Task<bool> UpdateMedicalRecordAsync(MedicalRecords record)
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                string query = @"
                UPDATE medical_records 
                SET icd_code = @icd, diagnosis = @diagnosis, treatment = @treatment, 
                    notes = @notes, pulse = @pulse, blood_pressure = @bp, 
                    temperature = @temp, spo2 = @spo2
                WHERE id = @id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", record.Id);
                    cmd.Parameters.AddWithValue("@icd", record.IcdCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@diagnosis", record.Diagnosis ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@treatment", record.Treatment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@notes", record.Notes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@pulse", record.Pulse ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@bp", record.BloodPressure ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@temp", record.Temperature ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@spo2", record.SPO2 ?? (object)DBNull.Value);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }
    }
}