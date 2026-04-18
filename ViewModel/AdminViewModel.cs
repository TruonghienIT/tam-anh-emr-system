using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public AdminViewModel()
        {
            LoadCurrentUserData();
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
