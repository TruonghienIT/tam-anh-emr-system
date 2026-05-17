using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel.Receptionist
{
    public class EditPatientViewModel : RegisterPatientViewModel
    {
        private readonly Patients _editingPatient;

        private readonly PatientRepository _repo;

        public override ICommand SubmitCommand =>
            new RelayCommand(async _ => await UpdatePatient());

        public override string SubmitButtonText =>
            "Cập nhật bệnh nhân";

        public EditPatientViewModel(Patients patient)
        {
            _editingPatient = patient;

            _repo = new PatientRepository();

            LoadPatient();
        }

        private void LoadPatient()
        {
            var names =
                _editingPatient.Name?.Split(' ');

            if (names?.Length > 1)
            {
                Patient.FirstName =
                    names[^1];

                Patient.LastName =
                    string.Join(" ", names, 0, names.Length - 1);
            }
            else
            {
                Patient.FirstName = _editingPatient.Name;
            }

            Patient.DateOfBirth = _editingPatient.Dob;

            Patient.Gender = _editingPatient.Gender;

            Patient.Phone = _editingPatient.Phone;

            Patient.Email = _editingPatient.Email;

            Patient.Address = _editingPatient.Address;

            Patient.BloodType = _editingPatient.BloodType;

            Patient.IdCard = _editingPatient.IdCard;

            Patient.Allergies = _editingPatient.Allergies;

            Patient.EmergencyContactName =
                _editingPatient.EmergencyContactName;

            Patient.EmergencyContactPhone =
                _editingPatient.EmergencyContactPhone;
        }

        private async Task UpdatePatient()
        {
            _editingPatient.Name =
                $"{Patient.LastName} {Patient.FirstName}";

            _editingPatient.Dob =
                Patient.DateOfBirth.Value;

            _editingPatient.Gender =
                Patient.Gender;

            _editingPatient.Phone =
                Patient.Phone;

            _editingPatient.Email =
                Patient.Email;

            _editingPatient.Address =
                Patient.Address;

            _editingPatient.BloodType =
                Patient.BloodType;

            _editingPatient.IdCard =
                Patient.IdCard;

            _editingPatient.Allergies =
                Patient.Allergies;

            _editingPatient.EmergencyContactName =
                Patient.EmergencyContactName;

            _editingPatient.EmergencyContactPhone =
                Patient.EmergencyContactPhone;

            await _repo.UpdateAsync(_editingPatient);

            MessageBox.Show(
                "Cập nhật bệnh nhân thành công"
            );
        }
    }
}