using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.Services
{
    /// <summary>
    /// Result returned by AppointmentRegistrationService after a registration attempt.
    /// </summary>
    public class RegistrationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string PatientId { get; set; }
        public string AppointmentId { get; set; }
        public bool IsExistingPatient { get; set; }

        public static RegistrationResult Success(string patientId, string appointmentId, bool isExisting)
            => new RegistrationResult
            {
                IsSuccess = true,
                Message = "Tạo lịch hẹn thành công!",
                PatientId = patientId,
                AppointmentId = appointmentId,
                IsExistingPatient = isExisting
            };

        public static RegistrationResult Failure(string message)
            => new RegistrationResult { IsSuccess = false, Message = message };
    }

    /// <summary>
    /// Service that orchestrates the full appointment registration flow
    /// within a single SqlTransaction:
    /// 
    /// 1. Check if patient already exists (by id_card or phone+dob+name)
    /// 2. If not found → create new patient with BN000001 format ID
    /// 3. Check doctor schedule conflict
    /// 4. Create appointment
    /// 5. COMMIT if all succeed, ROLLBACK on any failure
    /// 
    /// This is the single point of truth for the business rule:
    /// "Creating an appointment requires a valid patient record."
    /// </summary>
    public class AppointmentRegistrationService
    {
        private readonly PatientRepository _patientRepo;
        private readonly AppointmentRepository _appointmentRepo;

        public AppointmentRegistrationService()
        {
            _patientRepo = new PatientRepository();
            _appointmentRepo = new AppointmentRepository();
        }

        /// <summary>
        /// Registers a new appointment, creating the patient if needed.
        /// All operations run within a single SQL transaction.
        /// </summary>
        /// <param name="patient">Patient data (used for finding existing or creating new)</param>
        /// <param name="appointment">Appointment data to create</param>
        /// <returns>RegistrationResult with success/failure info</returns>
        public async Task<RegistrationResult> RegisterAppointmentAsync(
            Patients patient, Appointment appointment)
        {
            // Use the RepositoryBase's connection string via a new repo instance
            using (var conn = _patientRepo.GetPublicConnection())
            {
                await conn.OpenAsync();
                using (var txn = (SqlTransaction)await conn.BeginTransactionAsync())
                {
                    try
                    {
                        bool isExistingPatient = false;

                        // ===== STEP 1: Check if patient already exists =====
                        if (!string.IsNullOrWhiteSpace(patient.Id))
                        {
                            // Patient was selected from search — use their existing ID
                            isExistingPatient = true;
                            appointment.PatientId = patient.Id;
                        }
                        else
                        {
                            // Try to find existing by id_card or phone+dob+name
                            var existing = await FindExistingWithTransaction(
                                patient.IdCard, patient.Phone, patient.Dob, patient.Name,
                                conn, txn);

                            if (existing != null)
                            {
                                isExistingPatient = true;
                                patient.Id = existing.Id;
                                appointment.PatientId = existing.Id;
                            }
                            else
                            {
                                // ===== STEP 2: Create new patient =====
                                var newPatient = await _patientRepo.AddAsync(patient, conn, txn);
                                appointment.PatientId = newPatient.Id;
                                patient.Id = newPatient.Id;
                            }
                        }

                        // ===== STEP 3: Check doctor schedule conflict =====
                        bool hasConflict = await CheckConflictWithTransaction(
                            appointment.DoctorId, appointment.AppointmentDate,
                            appointment.AppointmentTime, conn, txn);

                        if (hasConflict)
                        {
                            await txn.RollbackAsync();
                            return RegistrationResult.Failure(
                                "Bác sĩ đã có lịch khám trong khung giờ này. Vui lòng chọn giờ khác.");
                        }

                        // ===== STEP 4: Create appointment =====
                        await _appointmentRepo.CreateAsync(appointment, conn, txn);

                        // ===== STEP 5: COMMIT =====
                        await txn.CommitAsync();

                        string msg = isExistingPatient
                            ? $"Đã tạo lịch hẹn cho bệnh nhân {patient.Name} (ID: {patient.Id})"
                            : $"Đã tạo bệnh nhân mới {patient.Name} (ID: {patient.Id}) và lịch hẹn thành công!";

                        return new RegistrationResult
                        {
                            IsSuccess = true,
                            Message = msg,
                            PatientId = patient.Id,
                            AppointmentId = appointment.Id,
                            IsExistingPatient = isExistingPatient
                        };
                    }
                    catch (Exception ex)
                    {
                        try { await txn.RollbackAsync(); } catch { /* rollback best effort */ }
                        return RegistrationResult.Failure($"Lỗi khi tạo lịch hẹn: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Find existing patient within the current transaction context.
        /// </summary>
        private async Task<Patients> FindExistingWithTransaction(
            string idCard, string phone, DateTime dob, string name,
            SqlConnection conn, SqlTransaction txn)
        {
            // Check by id_card first
            if (!string.IsNullOrWhiteSpace(idCard))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Transaction = txn;
                    cmd.CommandText = "SELECT * FROM patients WHERE id_card = @id_card";
                    cmd.Parameters.AddWithValue("@id_card", idCard);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapPatient(reader);
                    }
                }
            }

            // Check by phone + dob + name
            if (!string.IsNullOrWhiteSpace(phone) && !string.IsNullOrWhiteSpace(name))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Transaction = txn;
                    cmd.CommandText = @"
                        SELECT * FROM patients 
                        WHERE phone = @phone AND dob = @dob AND name = @name";
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@dob", dob);
                    cmd.Parameters.AddWithValue("@name", name);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapPatient(reader);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Check doctor schedule conflict within the current transaction context.
        /// </summary>
        private async Task<bool> CheckConflictWithTransaction(
            string doctorId, DateTime date, string timeSlot,
            SqlConnection conn, SqlTransaction txn)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = txn;
                cmd.CommandText = @"
                    SELECT COUNT(*) FROM appointments
                    WHERE doctor_id = @doctor_id 
                      AND appointment_date = @date 
                      AND appointment_time = @time_slot
                      AND status NOT IN (N'Đã hủy')";
                cmd.Parameters.AddWithValue("@doctor_id", doctorId);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@time_slot", timeSlot);

                var count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

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
