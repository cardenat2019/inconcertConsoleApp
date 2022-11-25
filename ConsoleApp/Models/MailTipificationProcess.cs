using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class MailTipificationProcess
    {
        public int Id { get; set; }
        public int TipProcessId { get; set; }
        public int MailConfigId { get; set; }
        public DateTime DateToSent { get; set; }
        public DateTime DateSent { get; set; }
        public Boolean mailSent { get; set; }

    }
}
