using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class ClientData
    {
        public int Id { get; set; }
        public string VCC { get; set; }
        public string Campaign { get; set; }
        public string StartDateTime { get; set; }
        public string EndDateTime { get; set; }
        public int RecordId { get; set; }
    }
}
