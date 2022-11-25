using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class BaseDataPost
    {
        public string IdOportunidad { get; set; }
        public string TotalLlamadas { get; set; }
        public string FechaLlamada { get; set; }
        public string Estatus1 { get; set; }
        public string Estatus2 { get; set; }
        public string Tipificacion { get; set; }
        public List<AdditionalDataPost> CamposAdicionales { get; set; }
    }
}
