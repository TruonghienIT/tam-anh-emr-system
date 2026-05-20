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

        public DateTime AppointmentDate { get; set; }
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
                        a.appointment_date,
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
                            DOB = reader["dob"] != DBNull.Value ? (DateTime)reader["dob"] : DateTime.Now,
                            AppointmentDate = reader["appointment_date"] != DBNull.Value ? (DateTime)reader["appointment_date"] : DateTime.Now,
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
            string labResult,
            List<TamAnh_EMR_System.Model.PrescriptionDetails> prescriptions = null)
            
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                SqlTransaction transaction =
                    connection.BeginTransaction();

                try
                {
                    string medicalRecordId =
                        await GenerateIdAsync(
                            connection,
                            transaction,
                            "medical_records",
                            "MR");

                    string labResultId =
                        await GenerateIdAsync(
                            connection,
                            transaction,
                            "lab_results",
                            "LR");

                    // =========================================
                    // INSERT MEDICAL RECORD
                    // =========================================

                    string insertMedicalRecord = @"
                INSERT INTO medical_records
                (
                    id,
                    appointment_id,
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
                    @appointmentId,
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

                    using (var cmd = new SqlCommand(
                        insertMedicalRecord,
                        connection,
                        transaction))
                    {
                        cmd.Parameters.AddWithValue( "@id", medicalRecordId);
                        cmd.Parameters.AddWithValue("@appointmentId", appointmentId);
                        cmd.Parameters.AddWithValue( "@patientId", patientId);
                        cmd.Parameters.AddWithValue( "@doctorId", doctorId);
                        cmd.Parameters.AddWithValue( "@icdCode", string.IsNullOrWhiteSpace(icdCode) ? DBNull.Value : icdCode);
                        cmd.Parameters.AddWithValue( "@diagnosis", diagnosis ?? "");
                        cmd.Parameters.AddWithValue( "@treatment", treatment ?? "");
                        cmd.Parameters.AddWithValue( "@notes", notes ?? "");
                        cmd.Parameters.AddWithValue( "@pulse", int.TryParse(pulse, out int p) ? p : DBNull.Value);
                        cmd.Parameters.AddWithValue( "@bloodPressure", string.IsNullOrWhiteSpace(bloodPressure) ? DBNull.Value : bloodPressure);
                        cmd.Parameters.AddWithValue( "@temperature", decimal.TryParse( temperature, out decimal t) ? t : DBNull.Value);
                        cmd.Parameters.AddWithValue( "@spo2", int.TryParse(spo2, out int s) ? s : DBNull.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // =========================================
                    // INSERT LAB RESULT
                    // =========================================

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

                        using (var cmd = new SqlCommand(
                            insertLab,
                            connection,
                            transaction))
                        {
                            cmd.Parameters.AddWithValue(
                                "@id",
                                labResultId);

                            cmd.Parameters.AddWithValue(
                                "@recordId",
                                medicalRecordId);

                            cmd.Parameters.AddWithValue(
                                "@testName",
                                labTestName);

                            cmd.Parameters.AddWithValue(
                                "@result",
                                labResult ?? "");

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    if (prescriptions != null && prescriptions.Any())
                    {
                        foreach (var item in prescriptions)
                        {
                            string prescriptionDetailId =
                                await GenerateIdAsync(
                                    connection,
                                    transaction,
                                    "prescription_details",
                                    "PD");

                            string insertPrescription = @"
            INSERT INTO prescription_details
            (
                id,
                record_id,
                medicine_id,
                quantity,
                dosage,
                frequency,
                notes
            )
            VALUES
            (
                @id,
                @recordId,
                @medicineId,
                @quantity,
                @dosage,
                @frequency,
                @notes
            )";

                            using (var cmd = new SqlCommand(insertPrescription, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", prescriptionDetailId);
                                cmd.Parameters.AddWithValue("@recordId", medicalRecordId);
                                cmd.Parameters.AddWithValue("@medicineId", item.MedicineId);
                                cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@dosage", item.Dosage ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@frequency", item.Frequency ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@notes", item.Notes ?? (object)DBNull.Value);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // =========================================
                    // UPDATE APPOINTMENT STATUS
                    // =========================================

                    string updateAppointment = @"
                UPDATE appointments
                SET status = N'Hoàn thành'
                WHERE id = @appointmentId";

                    using (var cmd = new SqlCommand(
                        updateAppointment,
                        connection,
                        transaction))
                    {
                        cmd.Parameters.AddWithValue(
                            "@appointmentId",
                            appointmentId);

                        await cmd.ExecuteNonQueryAsync();
                    }

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
                        MessageBoxImage.Error);

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
        // LẤY TẤT CẢ HỒ SƠ BỆNH ÁN (ĐÃ BỔ SUNG FULL THÔNG TIN BỆNH NHÂN)
        // =========================================================
        public async Task<List<MedicalRecords>> GetAllMedicalRecordsAsync()
        {
            var list = new List<MedicalRecords>();

            // Dùng Dictionary để nhóm các dòng dữ liệu trùng ID bệnh án lại với nhau
            var recordDictionary = new Dictionary<string, MedicalRecords>();

            using (var conn = GetConnection())
            {
                await conn.OpenAsync();

                // Bổ sung JOIN bảng prescription_details và medicines
                string query = @"
            SELECT
                mr.id, mr.patient_id, mr.doctor_id, mr.icd_code, mr.diagnosis,
                mr.treatment, mr.notes, mr.created_at, mr.pulse, mr.blood_pressure,
                mr.temperature, mr.spo2,
                p.name AS patient_name, p.gender, p.dob, p.phone, p.address,
                d.full_name AS doctor_name,
                ds.disease_name,
                lr.test_name, lr.result,
                pd.quantity, pd.frequency, pd.notes AS prescription_notes,
                m.name AS medicine_name, m.category
            FROM medical_records mr
            LEFT JOIN patients p ON mr.patient_id = p.id
            LEFT JOIN doctors d ON mr.doctor_id = d.id
            LEFT JOIN diseases ds ON mr.icd_code = ds.icd_code
            LEFT JOIN lab_results lr ON lr.record_id = mr.id
            LEFT JOIN prescription_details pd ON pd.record_id = mr.id
            LEFT JOIN medicines m ON pd.medicine_id = m.id
            ORDER BY mr.created_at DESC";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string recordId = reader["id"]?.ToString() ?? "";

                        // 1. KIỂM TRA BỆNH ÁN: Nếu chưa có trong Dictionary thì tạo mới
                        if (!recordDictionary.TryGetValue(recordId, out MedicalRecords currentRecord))
                        {
                            currentRecord = new MedicalRecords
                            {
                                Id = recordId,
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

                                Patient = new Patients
                                {
                                    Name = reader["patient_name"]?.ToString() ?? "N/A",
                                    Gender = reader["gender"]?.ToString() ?? "N/A",
                                    Phone = reader["phone"]?.ToString() ?? "N/A",
                                    Address = reader["address"]?.ToString() ?? "N/A",
                                    Dob = reader["dob"] != DBNull.Value ? (DateTime)reader["dob"] : DateTime.MinValue
                                },

                                Doctor = new Doctors { FullName = reader["doctor_name"]?.ToString() ?? "N/A" },
                                Disease = new Diseases { DiseaseName = reader["disease_name"]?.ToString() ?? "N/A" },

                                LabResults = new List<LabResults>(),
                                PrescriptionDetails = new List<PrescriptionDetails>()
                            };

                            // Map thông tin xét nghiệm (Chỉ làm 1 lần)
                            string labTestName = reader["test_name"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(labTestName))
                            {
                                currentRecord.LabResults.Add(new LabResults
                                {
                                    TestName = labTestName,
                                    Result = reader["result"]?.ToString()
                                });
                            }

                            recordDictionary.Add(recordId, currentRecord);
                            list.Add(currentRecord);
                        }

                        // 2. MAP THÔNG TIN THUỐC: Chạy nhiều lần nếu bệnh án có nhiều loại thuốc
                        string medName = reader["medicine_name"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(medName))
                        {
                            currentRecord.PrescriptionDetails.Add(new PrescriptionDetails
                            {
                                MedicineName = medName,
                                Category = reader["category"]?.ToString(),
                                Quantity = reader["quantity"] != DBNull.Value ? Convert.ToInt32(reader["quantity"]) : 0,
                                Frequency = reader["frequency"]?.ToString(),
                                Notes = reader["prescription_notes"]?.ToString()
                            });
                        }
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
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Cập nhật bảng medical_records
                    string queryMR = @"
            UPDATE medical_records 
            SET icd_code = @icd, diagnosis = @diagnosis, treatment = @treatment, 
                notes = @notes, pulse = @pulse, blood_pressure = @bp, 
                temperature = @temp, spo2 = @spo2
            WHERE id = @id";

                    using (var cmd = new SqlCommand(queryMR, conn, transaction))
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

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // 2. Cập nhật bảng lab_results
                    // SỬA: Dùng .Any() thay cho .Count > 0
                    if (record.LabResults != null && record.LabResults.Any())
                    {
                        // SỬA: Dùng FirstOrDefault() thay vì [0]
                        var lab = record.LabResults.FirstOrDefault();

                        if (lab != null)
                        {
                            // Kiểm tra xem bệnh án này đã có bản ghi lab_result chưa
                            string checkLabQuery = "SELECT COUNT(*) FROM lab_results WHERE record_id = @record_id";
                            int labCount = 0;
                            using (var checkCmd = new SqlCommand(checkLabQuery, conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@record_id", record.Id);
                                labCount = (int)await checkCmd.ExecuteScalarAsync();
                            }

                            if (labCount > 0)
                            {
                                // Nếu đã có -> Update
                                string updateLabQuery = @"
                        UPDATE lab_results 
                        SET test_name = @test_name, result = @result 
                        WHERE record_id = @record_id";
                                using (var cmdLab = new SqlCommand(updateLabQuery, conn, transaction))
                                {
                                    cmdLab.Parameters.AddWithValue("@record_id", record.Id);
                                    cmdLab.Parameters.AddWithValue("@test_name", lab.TestName ?? (object)DBNull.Value);
                                    cmdLab.Parameters.AddWithValue("@result", lab.Result ?? (object)DBNull.Value);
                                    await cmdLab.ExecuteNonQueryAsync();
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(lab.TestName))
                            {
                                // Nếu chưa có mà người dùng có nhập TestName -> Insert mới
                                string newLabId = await GenerateIdAsync(conn, transaction, "lab_results", "LR");
                                string insertLabQuery = @"
                        INSERT INTO lab_results (id, record_id, test_name, result, test_date)
                        VALUES (@id, @record_id, @test_name, @result, GETDATE())";

                                using (var cmdLab = new SqlCommand(insertLabQuery, conn, transaction))
                                {
                                    cmdLab.Parameters.AddWithValue("@id", newLabId);
                                    cmdLab.Parameters.AddWithValue("@record_id", record.Id);
                                    cmdLab.Parameters.AddWithValue("@test_name", lab.TestName);
                                    cmdLab.Parameters.AddWithValue("@result", lab.Result ?? (object)DBNull.Value);
                                    await cmdLab.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show(
                        $"Lỗi khi cập nhật bệnh án:\n{ex.Message}",
                        "Database Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            }
        }

        public async Task<MedicalRecords> GetMedicalRecordByAppointmentAsync(string appointmentId)
        {
            using var conn = GetConnection();

            await conn.OpenAsync();

            string query = @"
SELECT
    mr.*,
    ds.disease_name,
    lr.test_name,
    lr.result,

    pd.id AS prescription_detail_id,
    pd.medicine_id,
    pd.quantity,
    pd.dosage,
    pd.frequency,
    pd.notes AS prescription_notes,

    m.name AS medicine_name,
    m.category,
    m.unit,
    m.price,
    m.instruction
FROM medical_records mr

LEFT JOIN diseases ds
    ON mr.icd_code = ds.icd_code

LEFT JOIN lab_results lr
    ON lr.record_id = mr.id

LEFT JOIN prescription_details pd
    ON pd.record_id = mr.id

LEFT JOIN medicines m
    ON pd.medicine_id = m.id

WHERE mr.appointment_id = @appointmentId

ORDER BY mr.created_at DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@appointmentId", appointmentId);

            using var reader = await cmd.ExecuteReaderAsync();

            MedicalRecords record = null;

            while (await reader.ReadAsync())
            {
                if (record == null)
                {
                    record = new MedicalRecords
                    {
                        Id = reader["id"]?.ToString(),

                        PatientId = reader["patient_id"]?.ToString(),
                        DoctorId = reader["doctor_id"]?.ToString(),
                        IcdCode = reader["icd_code"]?.ToString(),

                        Diagnosis = reader["diagnosis"]?.ToString(),
                        Treatment = reader["treatment"]?.ToString(),
                        Notes = reader["notes"]?.ToString(),

                        Pulse = reader["pulse"]?.ToString(),
                        BloodPressure = reader["blood_pressure"]?.ToString(),
                        Temperature = reader["temperature"]?.ToString(),
                        SPO2 = reader["spo2"]?.ToString(),

                        CreatedAt = reader["created_at"] != DBNull.Value
                            ? Convert.ToDateTime(reader["created_at"])
                            : DateTime.MinValue,

                        Disease = new Diseases
                        {
                            DiseaseName = reader["disease_name"]?.ToString()
                        },

                        LabResults = new List<LabResults>(),
                        PrescriptionDetails = new List<PrescriptionDetails>()
                    };

                    string testName = reader["test_name"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(testName))
                    {
                        record.LabResults.Add(new LabResults
                        {
                            TestName = testName,
                            Result = reader["result"]?.ToString()
                        });
                    }
                }

                string medicineName = reader["medicine_name"]?.ToString();

                if (!string.IsNullOrWhiteSpace(medicineName))
                {
                    record.PrescriptionDetails.Add(new PrescriptionDetails
                    {
                        Id = reader["prescription_detail_id"]?.ToString(),
                        MedicineId = reader["medicine_id"]?.ToString(),
                        MedicineName = medicineName,
                        Category = reader["category"]?.ToString(),

                        Quantity = reader["quantity"] != DBNull.Value
                            ? Convert.ToInt32(reader["quantity"])
                            : 0,

                        Dosage = reader["dosage"]?.ToString(),
                        Frequency = reader["frequency"]?.ToString(),
                        Notes = reader["prescription_notes"]?.ToString()
                    });
                }
            }

            return record;
        }

        //public async Task<MedicalRecords> GetMedicalRecordByAppointmentAsync(string appointmentId)
        //{
        //    using var conn = GetConnection();

        //    await conn.OpenAsync();

        //    string query = @"
        //    SELECT TOP 1
        //        mr.*,
        //        ds.disease_name,
        //        lr.test_name,
        //        lr.result
        //    FROM medical_records mr

        //    LEFT JOIN diseases ds
        //        ON mr.icd_code = ds.icd_code

        //    LEFT JOIN lab_results lr
        //        ON lr.record_id = mr.id

        //    WHERE mr.appointment_id = @appointmentId

        //    ORDER BY mr.created_at DESC";

        //    using var cmd = new SqlCommand(query, conn);

        //    cmd.Parameters.AddWithValue(
        //        "@appointmentId",
        //        appointmentId);

        //    using var reader = await cmd.ExecuteReaderAsync();

        //    if (await reader.ReadAsync())
        //    {
        //        return new MedicalRecords
        //        {
        //            Id = reader["id"]?.ToString(),

        //            IcdCode = reader["icd_code"]?.ToString(),

        //            Diagnosis = reader["diagnosis"]?.ToString(),

        //            Treatment = reader["treatment"]?.ToString(),

        //            Notes = reader["notes"]?.ToString(),

        //            Pulse = reader["pulse"]?.ToString(),

        //            BloodPressure = reader["blood_pressure"]?.ToString(),

        //            Temperature = reader["temperature"]?.ToString(),

        //            SPO2 = reader["spo2"]?.ToString(),


        //            Disease = new Diseases
        //            {
        //                DiseaseName = reader["disease_name"]?.ToString()
        //            },

        //            LabResults = new List<LabResults>
        //    {
        //        new LabResults
        //        {
        //            TestName = reader["test_name"]?.ToString(),

        //            Result = reader["result"]?.ToString()
        //        }
        //    }
        //        };
        //    }

        //    return null;
        //}

        // =========================================================
        // TÌM KIẾM VÀ LỌC HỒ SƠ BỆNH ÁN (THEO NGÀY & TỪ KHÓA)
        // =========================================================
        public async Task<List<MedicalRecords>> SearchMedicalRecordsAsync(string keyword, DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<MedicalRecords>();

            using (var conn = GetConnection())
            {
                await conn.OpenAsync();

                string query = @"
                SELECT
                    mr.id, mr.patient_id, mr.doctor_id, mr.icd_code, mr.diagnosis,
                    mr.treatment, mr.notes, mr.created_at, mr.pulse, mr.blood_pressure,
                    mr.temperature, mr.spo2,
                    p.name AS patient_name, p.gender, p.dob, p.phone, p.address,
                    d.full_name AS doctor_name,
                    ds.disease_name,
                    lr.test_name, lr.result
                FROM medical_records mr
                LEFT JOIN patients p ON mr.patient_id = p.id
                LEFT JOIN doctors d ON mr.doctor_id = d.id
                LEFT JOIN diseases ds ON mr.icd_code = ds.icd_code
                LEFT JOIN lab_results lr ON lr.record_id = mr.id
                WHERE 1=1 ";

                // Điều kiện lọc theo từ khóa (Tìm theo Tên bệnh nhân, Mã ICD, hoặc Chẩn đoán)
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query += " AND (p.name LIKE @keyword OR mr.icd_code LIKE @keyword OR mr.diagnosis LIKE @keyword) ";
                }

                // Điều kiện lọc theo Từ ngày
                if (fromDate.HasValue)
                {
                    query += " AND CAST(mr.created_at AS DATE) >= @fromDate ";
                }

                // Điều kiện lọc theo Đến ngày
                if (toDate.HasValue)
                {
                    query += " AND CAST(mr.created_at AS DATE) <= @toDate ";
                }

                query += " ORDER BY mr.created_at DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                        cmd.Parameters.AddWithValue("@keyword", $"%{keyword.Trim()}%");

                    if (fromDate.HasValue)
                        cmd.Parameters.AddWithValue("@fromDate", fromDate.Value.Date);

                    if (toDate.HasValue)
                        cmd.Parameters.AddWithValue("@toDate", toDate.Value.Date);

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
                                Patient = new Patients
                                {
                                    Name = reader["patient_name"]?.ToString() ?? "N/A",
                                    Gender = reader["gender"]?.ToString() ?? "N/A",
                                    Phone = reader["phone"]?.ToString() ?? "N/A",
                                    Address = reader["address"]?.ToString() ?? "N/A",
                                    Dob = reader["dob"] != DBNull.Value ? (DateTime)reader["dob"] : DateTime.MinValue
                                },
                                Doctor = new Doctors { FullName = reader["doctor_name"]?.ToString() ?? "N/A" },
                                Disease = new Diseases { DiseaseName = reader["disease_name"]?.ToString() ?? "N/A" },
                                LabResults = new List<LabResults>
                        {
                            new LabResults
                            {
                                TestName = reader["test_name"]?.ToString(),
                                Result = reader["result"]?.ToString()
                            }
                        }
                            });
                        }
                    }
                }
            }
            return list;
        }

        public async Task<PatientQueueDTO> GetPatientByAppointmentIdAsync(string appointmentId)
        {
            using var connection = GetConnection();

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
                a.appointment_date,
                a.appointment_time,
                a.status,
                a.reason
            FROM appointments a
            JOIN patients p ON a.patient_id = p.id
            WHERE a.id = @appointmentId";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue( "@appointmentId", appointmentId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new PatientQueueDTO
                {
                    AppointmentId = reader["AppointmentId"]?.ToString(),
                    DoctorId = reader["DoctorId"]?.ToString(),
                    PatientId = reader["PatientId"]?.ToString(),
                    PatientName = reader["PatientName"]?.ToString(),
                    PhoneNumber = reader["PhoneNumber"]?.ToString(),
                    Gender = reader["gender"]?.ToString(),
                    DOB = reader["dob"] != DBNull.Value ? (DateTime)reader["dob"] : DateTime.Now,
                    AppointmentDate = reader["appointment_date"] != DBNull.Value ? (DateTime)reader["appointment_date"] : DateTime.Now,
                    BloodType = reader["blood_type"]?.ToString(),
                    AppointmentTime = reader["appointment_time"] != DBNull.Value ? (TimeSpan)reader["appointment_time"] : TimeSpan.Zero,
                    Status = reader["status"]?.ToString(),
                    Reason = reader["reason"]?.ToString()
                };
            }
            return null;
        }

        public async Task<List<Medicines>> GetMedicinesAsync()
        {
            var list = new List<Medicines>();

            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                // Câu lệnh SQL truy vấn danh mục thuốc của bạn
                string sql = "SELECT id, name, category, unit, price, instruction FROM medicines";

                using (var cmd = new SqlCommand(sql, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new Medicines
                        {
                            Id = r["id"].ToString(),
                            Name = r["name"].ToString(),
                            Category = r["category"]?.ToString(),
                            Unit = r["unit"]?.ToString(),
                            Price = r["price"] != DBNull.Value ? Convert.ToDecimal(r["price"]) : 0,
                            Instruction = r["instruction"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }
    }
}