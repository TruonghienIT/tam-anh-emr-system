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
            _connectionString = "Server=DESKTOP-T8N6JVU\\SQLEXPRESS;Database=hos_db;User Id=sa;Password=sa;TrustServerCertificate=True;";
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Public connection accessor for the service layer (transaction management).
        /// </summary>
        public SqlConnection GetPublicConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}