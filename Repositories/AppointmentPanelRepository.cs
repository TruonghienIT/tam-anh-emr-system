using System;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Repositories
{
    public class AppointmentPanelRepository : RepositoryBase
    {
        // ================= GET ALL =================
        public ObservableCollection<Appointment> GetAllAppointments()
        {
            var list = new ObservableCollection<Appointment>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    SELECT 
                        a.*,

                        p.name AS patient_name,

                        d.full_name AS doctor_name,

                        r.full_name AS receptionist_name

                    FROM appointments a

                    INNER JOIN patients p 
                        ON a.patient_id = p.id

                    INNER JOIN doctors d 
                        ON a.doctor_id = d.id

                    LEFT JOIN receptionists r
                        ON a.created_by = r.id";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Appointment
                        {
                            Id = reader["id"].ToString(),

                            PatientId = reader["patient_id"].ToString(),

                            DoctorId = reader["doctor_id"].ToString(),

                            CreatedBy = reader["created_by"]?.ToString(),

                            AppointmentDate =
                                Convert.ToDateTime(reader["appointment_date"]),

                            AppointmentTime =
                                reader["appointment_time"].ToString(),

                            Status =
                                reader["status"]?.ToString(),

                            Reason =
                                reader["reason"]?.ToString(),

                            Patient = new Patients
                            {
                                Name = reader["patient_name"].ToString()
                            },

                            Doctor = new Doctors
                            {
                                FullName = reader["doctor_name"].ToString()
                            },

                            Receptionist = new Receptionists
                            {
                                FullName = reader["receptionist_name"]?.ToString()
                            }
                        });
                    }
                }
            }

            return list;
        }

        // ================= DELETE =================
        public void DeleteAppointment(string id)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText =
                    "DELETE FROM appointments WHERE id=@id";

                command.Parameters.Add("@id", SqlDbType.VarChar)
                    .Value = id;

                command.ExecuteNonQuery();
            }
        }

        // ================= SEARCH =================
        public ObservableCollection<Appointment> SearchAppointments(string keyword)
        {
            var list = new ObservableCollection<Appointment>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = @"
                    SELECT 
                        a.*,

                        p.name AS patient_name,

                        d.full_name AS doctor_name,

                        r.full_name AS receptionist_name

                    FROM appointments a

                    INNER JOIN patients p 
                        ON a.patient_id = p.id

                    INNER JOIN doctors d 
                        ON a.doctor_id = d.id

                    LEFT JOIN receptionists r
                        ON a.created_by = r.id

                    WHERE
                        a.id LIKE @key
                        OR p.name LIKE @key
                        OR d.full_name LIKE @key
                        OR a.status LIKE @key
                        OR a.reason LIKE @key

                    ORDER BY a.appointment_date DESC";

                command.Parameters.Add("@key", SqlDbType.NVarChar)
                    .Value = "%" + keyword + "%";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Appointment
                        {
                            Id = reader["id"].ToString(),

                            PatientId = reader["patient_id"].ToString(),

                            DoctorId = reader["doctor_id"].ToString(),

                            CreatedBy = reader["created_by"]?.ToString(),

                            AppointmentDate =
                                Convert.ToDateTime(reader["appointment_date"]),

                            AppointmentTime =
                                reader["appointment_time"].ToString(),

                            Status =
                                reader["status"]?.ToString(),

                            Reason =
                                reader["reason"]?.ToString(),

                            Patient = new Patients
                            {
                                Name = reader["patient_name"].ToString()
                            },

                            Doctor = new Doctors
                            {
                                FullName = reader["doctor_name"].ToString()
                            },

                            Receptionist = new Receptionists
                            {
                                FullName = reader["receptionist_name"]?.ToString()
                            }
                        });
                    }
                }
            }

            return list;
        }
    }
}