using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class SmtpServerParameters
    {
        public int Id { get; set; }
        public int EnvironmentId { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public Boolean EnabledSsl { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
    }
}
