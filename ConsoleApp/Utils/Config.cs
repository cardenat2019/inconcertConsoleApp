using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Utils
{
    static class Config
    {
        public static string ConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["aliatConexion"].ConnectionString;
            return connectionString;
        }
        
    }
}
