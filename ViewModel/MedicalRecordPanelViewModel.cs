using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel
{
    public class MedicalRecordPanelViewModel : ViewModelBase
    {
        private readonly MedicalRecordPanelRepository _repository;

        public ObservableCollection<MedicalRecords> MedicalRecords { get; set; }

        private ObservableCollection<MedicalRecords> _allRecords;

        private MedicalRecords _selectedMedicalRecord;
        public MedicalRecords SelectedMedicalRecord
        {
            get => _selectedMedicalRecord;
            set
            {
                _selectedMedicalRecord = value;
                OnPropertyChanged(nameof(SelectedMedicalRecord));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));

                SearchMedicalRecords();
            }
        }
        
        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                _isPopupOpen = value;
                OnPropertyChanged(nameof(IsPopupOpen));
            }
        }

        private bool _isViewMode;
        public bool IsViewMode
        {
            get => _isViewMode;
            set
            {
                _isViewMode = value;
                OnPropertyChanged(nameof(IsViewMode));
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        private string _popupTitle;
        public string PopupTitle
        {
            get => _popupTitle;
            set
            {
                _popupTitle = value;
                OnPropertyChanged(nameof(PopupTitle));
            }
        }

        private MedicalRecords _currentMedicalRecord;
        public MedicalRecords CurrentMedicalRecord
        {
            get => _currentMedicalRecord;
            set
            {
                _currentMedicalRecord = value;
                OnPropertyChanged(nameof(CurrentMedicalRecord));
            }
        }

        public string SelectedIcdCode
        {
            get => CurrentMedicalRecord?.IcdCode;
            set
            {
                if (CurrentMedicalRecord == null) return;

                CurrentMedicalRecord.IcdCode = value;

                var disease = DiseaseList.FirstOrDefault(x => x.IcdCode == value);
                if (disease != null)
                {
                    CurrentMedicalRecord.Disease = new Diseases
                    {
                        IcdCode = disease.IcdCode,
                        DiseaseName = disease.DiseaseName
                    };
                }

                OnPropertyChanged(nameof(SelectedIcdCode));
                OnPropertyChanged(nameof(CurrentMedicalRecord));
                OnPropertyChanged(nameof(CurrentMedicalRecord.Disease));
            }
        }
        public ObservableCollection<Diseases> DiseaseList { get; set; }
        public ICommand ViewMedicalRecordCommand { get; }
        public ICommand EditMedicalRecordCommand { get; }
        public ICommand DeleteMedicalRecordCommand { get; }
        public ICommand SaveMedicalRecordCommand { get; }
        public ICommand ClosePopupCommand { get; }

        public MedicalRecordPanelViewModel()
        {
            _repository = new MedicalRecordPanelRepository();
            DiseaseList = new ObservableCollection<Diseases>(_repository.GetDiseases());
            LoadMedicalRecords();
            ViewMedicalRecordCommand = new RelayCommand(ViewMedicalRecord);
            EditMedicalRecordCommand = new RelayCommand(EditMedicalRecord);
            DeleteMedicalRecordCommand = new RelayCommand(DeleteMedicalRecord);
            SaveMedicalRecordCommand = new RelayCommand(SaveMedicalRecord);
            ClosePopupCommand = new RelayCommand(_ => IsPopupOpen = false);
        }

        private void ExecuteViewMedicalRecord(object obj)
        {
            if (obj is MedicalRecords record)
            {
                CurrentMedicalRecord = record;
                PopupTitle = "Chi tiết bệnh án";
                IsPopupOpen = true;
            }
        }
        private void ExecuteEditMedicalRecord(object obj)
        {
            if (obj is MedicalRecords record)
            {
                CurrentMedicalRecord = record;
                PopupTitle = "Chỉnh sửa bệnh án";
                IsPopupOpen = true;
            }
        }
        private void LoadMedicalRecords()
        {
            _allRecords = _repository.GetAllMedicalRecords();
            MedicalRecords =  new ObservableCollection<MedicalRecords>(_allRecords);
            OnPropertyChanged(nameof(MedicalRecords));
        }

        private void SearchMedicalRecords()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                MedicalRecords = new ObservableCollection<MedicalRecords>(_allRecords);
            else
            {
                string key = SearchText.ToLower();
                var filtered = _allRecords.Where(x =>
                    (x.Patient?.Name ?? "").ToLower().Contains(key) ||
                    (x.Doctor?.FullName ?? "").ToLower().Contains(key) ||
                    (x.IcdCode ?? "").ToLower().Contains(key) ||
                    (x.Disease?.DiseaseName ?? "").ToLower().Contains(key) ||
                    (x.Diagnosis ?? "").ToLower().Contains(key)
                );

                MedicalRecords = new ObservableCollection<MedicalRecords>(filtered);
            }
            OnPropertyChanged(nameof(MedicalRecords));
        }

        private void ViewMedicalRecord(object obj)
        {
            if (obj is not MedicalRecords record)
                return;

            CurrentMedicalRecord = CloneRecord(record);
            PopupTitle = "Chi tiết bệnh án";
            IsViewMode = true;
            IsEditMode = false;
            IsPopupOpen = true;
        }

        private void EditMedicalRecord(object obj)
        {
            if (obj is not MedicalRecords record)
                return;

            CurrentMedicalRecord = CloneRecord(record);
            PopupTitle = "Cập nhật bệnh án";
            IsViewMode = false;
            IsEditMode = true;
            IsPopupOpen = true;
        }

        private void SaveMedicalRecord(object obj)
        {
            if (CurrentMedicalRecord == null)
                return;
            try
            {
                _repository.UpdateMedicalRecord(CurrentMedicalRecord);
                MessageBox.Show( "Cập nhật bệnh án thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadMedicalRecords();
                IsPopupOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteMedicalRecord(object obj)
        {
            if (obj is not MedicalRecords record)
                return;

            var result = MessageBox.Show( $"Bạn có chắc muốn xóa bệnh án của {record.Patient?.Name} ?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _repository.DeleteMedicalRecord(record.Id);

                MedicalRecords.Remove(record);

                MessageBox.Show( "Xóa bệnh án thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private MedicalRecords CloneRecord(MedicalRecords r)
        {
            return new MedicalRecords
            {
                Id = r.Id,
                PatientId = r.PatientId,
                DoctorId = r.DoctorId,
                IcdCode = r.IcdCode,
                Diagnosis = r.Diagnosis,
                Treatment = r.Treatment,
                Notes = r.Notes,
                Pulse = r.Pulse,
                BloodPressure = r.BloodPressure,
                Temperature = r.Temperature,
                SPO2 = r.SPO2,
                CreatedAt = r.CreatedAt,
                Patient = r.Patient,
                Doctor = r.Doctor,
                Disease = r.Disease,
                LabResults = r.LabResults?
                .Select(x => new LabResults
                {
                    TestName = x.TestName,
                    Result = x.Result
                })
                .ToList() ?? new List<LabResults>()
            };
        }
    }
}