using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class ResponseService
    {
        public string Mensaje { get; set; }
        public List<ResponseServiceDetail> Detalles { get; set; }
    }
}
