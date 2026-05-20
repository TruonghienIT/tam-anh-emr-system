using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel
{
    public class PrescriptionPanelViewModel : ViewModelBase
    {
        private readonly PrescriptionPanelRepository _repository;
        public ObservableCollection<PrescriptionGroup> Prescriptions { get; set; }

        private ObservableCollection<PrescriptionGroup> _allPrescriptions;

        private PrescriptionGroup _selectedPrescription;
        public PrescriptionGroup SelectedPrescription
        {
            get => _selectedPrescription;
            set
            {
                _selectedPrescription = value;
                OnPropertyChanged(nameof(SelectedPrescription));
            }
        }

        private PrescriptionGroup _currentPrescription;
        public PrescriptionGroup CurrentPrescription
        {
            get => _currentPrescription;
            set
            {
                _currentPrescription = value;
                OnPropertyChanged(nameof(CurrentPrescription));
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

                SearchPrescriptions();
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
        public ICommand ViewPrescriptionCommand { get; }
        public ICommand EditPrescriptionCommand { get; }
        public ICommand DeletePrescriptionCommand { get; }
        public ICommand SavePrescriptionCommand { get; }
        public ICommand ClosePopupCommand { get; }

        public PrescriptionPanelViewModel()
        {
            _repository = new PrescriptionPanelRepository();
            LoadPrescriptions();
            ViewPrescriptionCommand = new RelayCommand(ViewPrescription);
            EditPrescriptionCommand = new RelayCommand(EditPrescription);
            DeletePrescriptionCommand = new RelayCommand(DeletePrescription);
            SavePrescriptionCommand = new RelayCommand(SavePrescription);
            ClosePopupCommand = new RelayCommand(_ => IsPopupOpen = false);
        }

        private void LoadPrescriptions()
        {
            _allPrescriptions = _repository.GetAllPrescriptions();
            Prescriptions = new ObservableCollection<PrescriptionGroup>( _allPrescriptions);
            OnPropertyChanged(nameof(Prescriptions));
        }

        private void SearchPrescriptions()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                Prescriptions = new ObservableCollection<PrescriptionGroup>( _allPrescriptions);
            else
            {
                string key = SearchText.ToLower();
                var filtered = _allPrescriptions.Where(x => 
                    (x.RecordId ?? "").ToLower().Contains(key)
                    ||
                    (x.MedicalRecord?.Patient?.Name ?? "").ToLower().Contains(key)
                    ||
                    (x.MedicalRecord?.Doctor?.FullName ?? "").ToLower().Contains(key)
                    ||
                    x.PrescriptionDetails.Any(p => (p.Medicine?.Name ?? "").ToLower().Contains(key))
                );
                Prescriptions = new ObservableCollection<PrescriptionGroup>(filtered);
            }
            OnPropertyChanged(nameof(Prescriptions));
        }

        private void ViewPrescription(object obj)
        {
            if (obj is not PrescriptionGroup prescription) return;
            CurrentPrescription = prescription;
            PopupTitle = "Chi tiết đơn thuốc";
            IsViewMode = true;
            IsEditMode = false;
            IsPopupOpen = true;
        }

        private void EditPrescription(object obj)
        {
            if (obj is not PrescriptionGroup prescription) return;
            CurrentPrescription = prescription;
            PopupTitle = "Cập nhật đơn thuốc";
            IsViewMode = false;
            IsEditMode = true;
            IsPopupOpen = true;
        }

        private void SavePrescription(object obj)
        {
            if (CurrentPrescription == null) return;

            try
            {
                foreach (var item in CurrentPrescription.PrescriptionDetails)
                {
                    _repository.UpdatePrescription(item);
                }    

                MessageBox.Show( "Cập nhật đơn thuốc thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPrescriptions();
                IsPopupOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeletePrescription(object obj)
        {
            if (obj is not PrescriptionGroup prescription) return;
            var result = MessageBox.Show($"Bạn có chắc muốn xóa đơn thuốc của bệnh án {prescription.RecordId} ?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _repository.DeletePrescription( prescription.RecordId);
                Prescriptions.Remove(prescription);
                MessageBox.Show( "Xóa đơn thuốc thành công!", "Thông báo", MessageBoxButton.OK,MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}