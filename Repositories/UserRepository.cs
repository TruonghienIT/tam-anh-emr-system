using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TamAnh_EMR_System.Model;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TamAnh_EMR_System.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {
        public void Add(Users user)
        {
            throw new NotImplementedException();
        }

        public Users AuthenticateUser(NetworkCredential credential)
        {
            Users user = null;

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "SELECT * FROM [Users] WHERE username=@username AND [password]=@password";

                command.Parameters.Add("@username", SqlDbType.VarChar).Value = credential.UserName;
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = credential.Password;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new Users
                        {
                            Id = reader["id"].ToString(),
                            Username = reader["username"].ToString(),
                            Role = reader["role"].ToString()
                        };
                    }
                }
            }

            return user;
        }

        public void Edit(Users user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Users> GetByAll()
        {
            throw new NotImplementedException();
        }

        public Users GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Users GetByUsername(string username)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
