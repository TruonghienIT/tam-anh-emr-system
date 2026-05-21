using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    /// <summary>
    /// Interface for Appointment data access.
    /// Write methods accept SqlConnection+SqlTransaction for transactional support.
    /// </summary>
    public interface IAppointmentRepository
    {
        /// <summary>
        /// Inserts a new appointment into the database.
        /// Uses the provided connection/transaction for transactional support.
        /// </summary>
        Task CreateAsync(Appointment appointment, SqlConnection conn, SqlTransaction txn);

        /// <summary>
        /// Gets appointments for the dashboard table.
        /// JOINs appointments + patients + doctors for display-ready data.
        /// </summary>
        Task<List<DashboardAppointment>> GetDashboardAppointmentsAsync(DateTime? date = null);

        /// <summary>
        /// Checks if a doctor already has an appointment at the given date+time.
        /// Returns true if there is a schedule conflict.
        /// </summary>
        Task<bool> CheckDoctorScheduleConflictAsync(string doctorId, DateTime date, string timeSlot);

        Task<bool> HasPatientAppointmentSameDayAsync(
            string patientId,
            DateTime date);

        Task<bool> HasPatientAppointmentSameWeekAsync(
            string patientId,
            DateTime date);
        /// <summary>
        /// Generates the next appointment ID in AP000001 format.
        /// </summary>
        Task<string> GenerateNextIdAsync(SqlConnection conn, SqlTransaction txn);

        /// <summary>
        /// Gets count of appointments by status for today (for statistic cards).
        /// </summary>
        Task<Dictionary<string, int>> GetTodayStatisticsAsync();
        Task<List<AppointmentDisplay>> GetAllDisplayAsync();
        Task<List<string>> GetDoctorBusySlotsAsync(
            string doctorId,
            DateTime date);
        Task UpdateStatusAsync(string id, string status);

        Task DeleteAsync(string id);
    }
}
