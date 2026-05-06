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
using TamAnh_EMR_System.Services;
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

        private bool _isResetMode;
        public bool IsResetMode
        {
            get => _isResetMode;
            set
            {
                _isResetMode = value;
                OnPropertyChanged(nameof(IsResetMode));
            }
        }

        private string _resetEmail;
        public string ResetEmail
        {
            get => _resetEmail;
            set
            {
                _resetEmail = value;
                OnPropertyChanged(nameof(ResetEmail));
            }
        }

        private bool _isSending;
        public bool IsSending
        {
            get => _isSending;
            set
            {
                _isSending = value;
                OnPropertyChanged(nameof(IsSending));
            }
        }

        private DateTime _lastSendTime = DateTime.MinValue;

        private int _cooldownSeconds;
        public int CooldownSeconds
        {
            get => _cooldownSeconds;
            set
            {
                _cooldownSeconds = value;
                OnPropertyChanged(nameof(CooldownSeconds));
            }
        }

        public ICommand ShowResetCommand { get; }
        public ICommand BackToLoginCommand { get; }
        public ICommand SendResetCommand { get; }

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

        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RememberPasswordCommand { get; }

        public LoginViewModel()
        {
            userRepository = new UserRepository();

            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);

            RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));

            ShowResetCommand = new ViewModelCommand(_ =>
            {
                IsResetMode = true;
                ErrorMessage = "";
            });

            BackToLoginCommand = new ViewModelCommand(_ =>
            {
                IsResetMode = false;
                ResetEmail = "";
            });

            SendResetCommand = new ViewModelCommand(_ => ExecuteSendReset());
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
                    new GenericIdentity(user.Username),
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

        private string GenerateGuidPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }
        private async void ExecuteSendReset()
        {
            if (IsSending) return;

            if ((DateTime.Now - _lastSendTime).TotalSeconds < 30)
            {
                int remain = 30 - (int)(DateTime.Now - _lastSendTime).TotalSeconds;
                MessageBox.Show($"Vui lòng chờ {remain}s trước khi gửi lại!");
                return;
            }

            if (string.IsNullOrWhiteSpace(ResetEmail))
            {
                MessageBox.Show("Vui lòng nhập email!");
                return;
            }

            if (!ResetEmail.Contains("@"))
            {
                MessageBox.Show("Email không hợp lệ!");
                return;
            }

            var user = userRepository.GetByEmail(ResetEmail);

            if (user == null)
            {
                MessageBox.Show("Không tìm thấy email!");
                return;
            }

            string newPassword = GenerateGuidPassword();

            user.Password = newPassword;
            userRepository.Edit(user);

            try
            {
                IsSending = true;

                var emailService = new EmailService();

                await emailService.SendResetPasswordEmailAsync(
                    ResetEmail,
                    user.Username,
                    newPassword
                );

                MessageBox.Show("Mật khẩu mới đã được gửi qua email!");

                _lastSendTime = DateTime.Now;
                StartCooldownTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi mail: " + ex.Message);
            }
            finally
            {
                IsSending = false;
            }

            IsResetMode = false;
            ResetEmail = "";
        }
        private async void StartCooldownTimer()
        {
            CooldownSeconds = 30;

            while (CooldownSeconds > 0)
            {
                await Task.Delay(1000);
                CooldownSeconds--;
            }
        }
    }
}