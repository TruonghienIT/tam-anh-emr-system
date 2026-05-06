using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace TamAnh_EMR_System.Model
{
    public interface IUserRepository
    {
        Users AuthenticateUser(NetworkCredential credential);
        void Add(Users user);
        void Edit(Users user);
        void Remove (int id);
        Users GetById(int id);
        Users GetByUsername (string username);
        IEnumerable<Users> GetByAll();

        Users GetByEmail(string email);

    }
}
