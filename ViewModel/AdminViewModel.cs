using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Helper;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.View;

namespace TamAnh_EMR_System.ViewModel
{
    public class AdminViewModel : ViewModelBase
    {
        private string _username;
        private string _role;

        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged(nameof(Role));
            }
        }

        public ViewModelBase CurrentChildView 
        { 
            get => _currentChildView; 
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }
        public string Caption 
        { 
            get => _caption; 
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public IconChar Icon 
        { 
            get => _icon; 
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }    
        }

        //Commands
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowUserPanelViewCommand { get; }
        public ICommand ShowDoctorPanelViewCommand { get; }
        public ICommand ShowReceptionistPanelViewCommand { get; }
        public ICommand ShowPatientPanelViewCommand { get; }
        public ICommand ShowAppointmentPanelViewCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminViewModel()
        {
            //Initialize commands
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowUserPanelViewCommand = new ViewModelCommand(ExecuteShowUserViewCommand);
            ShowDoctorPanelViewCommand = new ViewModelCommand(ExecuteShowDoctorViewCommand);
            ShowReceptionistPanelViewCommand = new ViewModelCommand(ExecuteShowReceptionistViewCommand);
            ShowPatientPanelViewCommand = new ViewModelCommand(ExecuteShowPatientViewCommand);
            ShowAppointmentPanelViewCommand = new ViewModelCommand(ExecuteShowAppointmentViewCommand);
            LogoutCommand = new ViewModelCommand(ExecuteLogoutCommand);

            //Default View
            ExecuteShowHomeViewCommand(null);

            LoadCurrentUserData();
        }

        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new HomeViewModel();
            Caption = "Trang chủ";
            Icon = IconChar.Home;
        }
        private void ExecuteShowUserViewCommand(object obj)
        {
            CurrentChildView = new UserPanelViewModel();
            Caption = "Tài khoản";
            Icon = IconChar.CircleUser;
        }
        private void ExecuteShowDoctorViewCommand(object obj)
        {
            CurrentChildView = new DoctorPanelViewModel();
            Caption = "Bác sĩ";
            Icon = IconChar.UserDoctor;
        }

        private void ExecuteShowReceptionistViewCommand(object obj)
        {
            CurrentChildView = new ReceptionistPanelViewModel();
            Caption = "Lễ tân";
            Icon = IconChar.UserAstronaut;
        }

        private void ExecuteShowPatientViewCommand(object obj)
        {
            CurrentChildView = new PatientPanelViewModel();
            Caption = "Bệnh nhân";
            Icon = IconChar.Bed;
        }
        public void ExecuteShowAppointmentViewCommand (object obj)
        {
            CurrentChildView = new AppointmentPanelViewModel();
            Caption = "Lịch hẹn";
            Icon = IconChar.ClipboardList;
        }
        private void ExecuteLogoutCommand(object obj)
        {
            var result = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginView loginView = new LoginView();
                loginView.Show();
                Application.Current.Windows
                    .OfType<Window>()
                    .SingleOrDefault(w => w is AdminView)
                    ?.Close();
            }
        }
        private void LoadCurrentUserData()
        {
            var user = UserSession.CurrentUser;
            if(user != null)
            {
                Username = user.Username;
                Role = user.Role;
            }    
        }
    }
}
