using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Principal;
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
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        private SecureString _password;
        private string _errorMessage;
        private bool _isViewVisible = true;

        private IUserRepository userRepository;
        public string Username 
        { 
            get => _username; 
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        public SecureString Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        public string ErrorMessage 
        { 
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public  ICommand LoginCommand
        {
            get;
        }
        public ICommand RecoverPasswordCommand
        {
            get;
        }
        public ICommand ShowPasswordCommand
        {
            get;
        }
        public ICommand RememberPasswordCommand
        {
            get;
        }

        public LoginViewModel()
        {
            userRepository = new UserRepository();
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            bool valiData;
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 ||
                Password == null || Password.Length < 3)
                valiData = false;
            else valiData = true;
            return valiData;
        }

        private void ExecuteLoginCommand(object obj)
        {
            var user = userRepository.AuthenticateUser(new NetworkCredential(Username, Password));
            if (user != null)
            {
                UserSession.CurrentUser = user;

                Thread.CurrentPrincipal = new GenericPrincipal( 
                    new GenericIdentity (user.Username), 
                    new[] { user.Role });

                Window window = null;

                switch (user.Role.Trim().ToLower())
                {
                    case "admin":
                        window = new AdminView();
                        break;

                    case "doctor":
                        window = new DoctorView();
                        break;

                    case "receptionist":
                        window = new ReceptionistView();
                        break;

                    default:
                        MessageBox.Show("Role không hợp lệ!");
                        return;
                }

                if (window == null)
                {
                    MessageBox.Show("Role không hợp lệ!");
                    return;
                }
                window.Show();

                Application.Current.MainWindow = window;

                if (obj is Window loginWindow)
                {
                    loginWindow.Close();
                }
            }
            else
            {
                ErrorMessage = "* Tài khoản || Mật khẩu không hợp lệ";
            }
        }

        private void ExecuteRecoverPassCommand(string username, string email)
        {
            throw new NotImplementedException();
        }
    }
}
