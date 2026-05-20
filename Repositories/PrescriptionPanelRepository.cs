using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    public class PrescriptionPanelRepository : RepositoryBase
    {
        public ObservableCollection<PrescriptionGroup> GetAllPrescriptions()
        {
            var dictionary = new Dictionary<string, PrescriptionGroup>();

            using var connection = GetConnection();

            connection.Open();

            using var command = new SqlCommand(@"
                SELECT
                    pd.id,
                    pd.record_id,
                    pd.medicine_id,
                    pd.quantity,
                    pd.dosage,
                    pd.frequency,
                    pd.notes,

                    m.name AS medicine_name,
                    m.category,
                    m.unit,
                    m.price,
                    m.instruction,
                    mr.diagnosis,
                    mr.created_at,

                    p.name AS patient_name,
                    d.full_name AS doctor_name

                FROM prescription_details pd

                LEFT JOIN medicines m
                    ON pd.medicine_id = m.id

                LEFT JOIN medical_records mr
                    ON pd.record_id = mr.id

                LEFT JOIN patients p
                    ON mr.patient_id = p.id

                LEFT JOIN doctors d
                    ON mr.doctor_id = d.id

                ORDER BY mr.created_at DESC
            ", connection);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                string recordId = reader["record_id"]?.ToString();

                if (!dictionary.ContainsKey(recordId))
                {
                    dictionary[recordId] = new PrescriptionGroup
                    {
                        RecordId = recordId,

                        CreatedAt = reader["created_at"] != DBNull.Value
                            ? Convert.ToDateTime(reader["created_at"])
                            : DateTime.Now,

                        MedicalRecord = new MedicalRecords
                        {
                            Id = recordId,

                            Diagnosis = reader["diagnosis"]?.ToString(),

                            Patient = new Patients
                            {
                                Name = reader["patient_name"]?.ToString()
                            },

                            Doctor = new Doctors
                            {
                                FullName = reader["doctor_name"]?.ToString()
                            }
                        }
                    };
                }

                dictionary[recordId]
                    .PrescriptionDetails
                    .Add(new PrescriptionDetails
                    {
                        Id = reader["id"]?.ToString(),

                        RecordId = recordId,

                        MedicineId = reader["medicine_id"]?.ToString(),

                        Quantity = reader["quantity"] != DBNull.Value
                            ? Convert.ToInt32(reader["quantity"])
                            : 0,

                        Dosage = reader["dosage"]?.ToString(),

                        Frequency = reader["frequency"]?.ToString(),

                        Notes = reader["notes"]?.ToString(),

                        Medicine = new Medicines
                        {
                            Id = reader["medicine_id"]?.ToString(),

                            Name = reader["medicine_name"]?.ToString(),

                            Category = reader["category"]?.ToString(),

                            Unit = reader["unit"]?.ToString(),

                            Price = reader["price"] != DBNull.Value
                                ? Convert.ToDecimal(reader["price"])
                                : 0,

                            Instruction = reader["instruction"]?.ToString()
                        }
                    });
            }

            return new ObservableCollection<PrescriptionGroup>(
                dictionary.Values);
        }

        public void UpdatePrescription(PrescriptionDetails prescription)
        {
            using var connection = GetConnection();

            connection.Open();

            using var command = new SqlCommand(@"
                UPDATE prescription_details
                SET
                    quantity = @quantity,
                    dosage = @dosage,
                    frequency = @frequency,
                    notes = @notes
                WHERE id = @id
            ", connection);

            command.Parameters.AddWithValue("@id", prescription.Id);

            command.Parameters.AddWithValue("@quantity",
                prescription.Quantity);

            command.Parameters.AddWithValue("@dosage",
                (object?)prescription.Dosage ?? DBNull.Value);

            command.Parameters.AddWithValue("@frequency",
                (object?)prescription.Frequency ?? DBNull.Value);

            command.Parameters.AddWithValue("@notes",
                (object?)prescription.Notes ?? DBNull.Value);

            command.ExecuteNonQuery();
        }

        public void DeletePrescription(string recordId)
        {
            using var connection = GetConnection();

            connection.Open();

            using var command = new SqlCommand(@"
                DELETE FROM prescription_details
                WHERE record_id = @record_id
            ", connection);

            command.Parameters.AddWithValue("@record_id", recordId);

            command.ExecuteNonQuery();
        }
    }
}