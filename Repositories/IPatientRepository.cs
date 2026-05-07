using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    /// <summary>
    /// Interface for Patient data access.
    /// All write methods accept optional SqlConnection+SqlTransaction so they can
    /// participate in an external transaction managed by the service layer.
    /// </summary>
    public interface IPatientRepository
    {
        /// <summary>
        /// Inserts a new patient (standalone, self-managed connection).
        /// Generates ID automatically. Returns patient with generated ID.
        /// </summary>
        Task<Patients> AddAsync(Patients patient);

        /// <summary>
        /// Inserts a new patient using an external connection/transaction.
        /// For use in cross-repository transactions (e.g., appointment registration).
        /// </summary>
        Task<Patients> AddAsync(Patients patient, SqlConnection conn, SqlTransaction txn);

        /// <summary>
        /// Finds an existing patient by id_card first, then by phone+dob+name combo.
        /// Returns null if no match found.
        /// </summary>
        Task<Patients> FindExistingAsync(string idCard, string phone, DateTime? dob, string name);

        /// <summary>
        /// Checks if a patient with the given phone already exists.
        /// </summary>
        Task<bool> ExistsByPhoneAsync(string phone);

        /// <summary>
        /// Searches patients by keyword (matches name, phone, id, or id_card).
        /// </summary>
        Task<List<Patients>> SearchAsync(string keyword);

        /// <summary>
        /// Gets all patients from the database.
        /// </summary>
        Task<List<Patients>> GetAllAsync();

        /// <summary>
        /// Gets a patient by their ID (e.g., "BN000001").
        /// </summary>
        Task<Patients> GetByIdAsync(string patientId);

        /// <summary>
        /// Generates the next patient ID in BN000001 format.
        /// </summary>
        Task<string> GenerateNextIdAsync(SqlConnection conn, SqlTransaction txn);
        Task<string> GenerateNextIdAsync();
    }
}
