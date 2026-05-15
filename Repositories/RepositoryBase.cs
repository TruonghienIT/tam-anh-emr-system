using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace TamAnh_EMR_System.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly string _connectionString;
        public RepositoryBase()
        {
            _connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public SqlConnection GetPublicConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}