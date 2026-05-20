using System;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    public class MedicalRecordPanelRepository : RepositoryBase
    {
        public ObservableCollection<MedicalRecords> GetAllMedicalRecords()
        {
            var dict = new Dictionary<string, MedicalRecords>();
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = @"
                SELECT
                    mr.*,
                    p.name AS patient_name,
                    d.full_name AS doctor_name,
                    ds.disease_name,
                    lr.test_name,
                    lr.result
                FROM medical_records mr
                LEFT JOIN patients p ON mr.patient_id = p.id
                LEFT JOIN doctors d ON mr.doctor_id = d.id
                LEFT JOIN diseases ds ON mr.icd_code = ds.icd_code
                LEFT JOIN lab_results lr ON mr.id = lr.record_id
                ORDER BY mr.created_at DESC";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader["id"].ToString();

                        if (!dict.ContainsKey(id))
                        {
                            dict[id] = new MedicalRecords
                            {
                                Id = id,
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
                                    : DateTime.Now,
                                Patient = new Patients
                                {
                                    Name = reader["patient_name"]?.ToString()
                                },

                                Doctor = new Doctors
                                {
                                    FullName = reader["doctor_name"]?.ToString()
                                },

                                Disease = new Diseases
                                {
                                    DiseaseName = reader["disease_name"]?.ToString()
                                },
                                LabResults = new List<LabResults>()
                            };
                        }

                        if (reader["test_name"] != DBNull.Value)
                        {
                            dict[id].LabResults.Add(new LabResults
                            {
                                TestName = reader["test_name"]?.ToString(),
                                Result = reader["result"]?.ToString()
                            });
                        }
                    }
                }
            }

            return new ObservableCollection<MedicalRecords>(dict.Values);
        }

        public void UpdateMedicalRecord(MedicalRecords record)
        {
            using var connection = GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using (var command = new SqlCommand(@"
                UPDATE medical_records
                SET icd_code = @icd_code,
                    diagnosis = @diagnosis,
                    treatment = @treatment,
                    notes = @notes,
                    pulse = @pulse,
                    blood_pressure = @blood_pressure,
                    temperature = @temperature,
                    spo2 = @spo2
                WHERE id = @id", connection, transaction))
                {
                    command.Parameters.AddWithValue("@id", record.Id);
                    command.Parameters.AddWithValue( "@icd_code", (object?)record.IcdCode ?? DBNull.Value);
                    command.Parameters.AddWithValue( "@diagnosis", (object?)record.Diagnosis ?? DBNull.Value);
                    command.Parameters.AddWithValue( "@treatment", (object?)record.Treatment ?? DBNull.Value);
                    command.Parameters.AddWithValue( "@notes", (object?)record.Notes ?? DBNull.Value);
                    command.Parameters.AddWithValue( "@pulse", (object?)record.Pulse ?? DBNull.Value);
                    command.Parameters.AddWithValue( "@blood_pressure", (object?)record.BloodPressure ?? DBNull.Value);
                    command.Parameters.AddWithValue( "@temperature", (object?)record.Temperature ?? DBNull.Value);
                    command.Parameters.AddWithValue( "@spo2", (object?)record.SPO2 ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }

                var lab = record.LabResults?.FirstOrDefault();
                if (lab != null)
                {
                    string checkQuery = @" SELECT COUNT(*) FROM lab_results WHERE record_id = @record_id";
                    int count = 0;
                    using (var checkCmd = new SqlCommand(checkQuery, connection, transaction))
                    {
                        checkCmd.Parameters.AddWithValue("@record_id", record.Id);
                        count = (int)checkCmd.ExecuteScalar();
                    }
                    if (count > 0)
                    {
                        using var updateLabCmd = new SqlCommand(@" UPDATE lab_results SET test_name = @test_name, result = @result WHERE record_id = @record_id", connection, transaction);
                        updateLabCmd.Parameters.AddWithValue("@record_id", record.Id);
                        updateLabCmd.Parameters.AddWithValue("@test_name", (object?)lab.TestName ?? DBNull.Value);
                        updateLabCmd.Parameters.AddWithValue("@result", (object?)lab.Result ?? DBNull.Value);
                        updateLabCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        using var insertLabCmd = new SqlCommand(@" INSERT INTO lab_results ( record_id, test_name, result) VALUES (@record_id, @test_name, @result)", connection, transaction);
                        insertLabCmd.Parameters.AddWithValue( "@record_id", record.Id);
                        insertLabCmd.Parameters.AddWithValue( "@test_name", (object?)lab.TestName ?? DBNull.Value);
                        insertLabCmd.Parameters.AddWithValue( "@result", (object?)lab.Result ?? DBNull.Value);
                        insertLabCmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }


        public void DeleteMedicalRecord(string id)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "DELETE FROM medical_records WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.VarChar).Value = id;
                command.ExecuteNonQuery();
            }
        }

        public List<Diseases> GetDiseases()
        {
            var list = new List<Diseases>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT icd_code, disease_name FROM diseases", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Diseases
                {
                    IcdCode = reader["icd_code"].ToString(),
                    DiseaseName = reader["disease_name"].ToString()
                });
            }
            return list;
        }
    }
}