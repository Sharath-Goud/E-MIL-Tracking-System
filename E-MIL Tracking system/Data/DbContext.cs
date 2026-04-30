using Microsoft.Data.SqlClient;

namespace E_MIL_Tracking_system.Data
{
    public class DbContext
    {
        private readonly IConfiguration _config;

        public DbContext(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }
    }
}
