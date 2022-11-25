using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class ValoresResultados
    {
        public String IdContacto { get; set; }
        public String IdOportunidad { get; set; }
        public int TotalLlamadas { get; set; }
        public DateTime FechaLlamada { get; set; }
        public String Estatus_1 { get; set; }
        public String Estatus_2 { get; set; }
        public String Tipificacion { get; set; }
        public int ValorTipificacion { get; set; }
        public String VCC { get; set; }
        public String Campana { get; set; }
        public DateTime scheduledDate { get; set; }
        public DateTime CuandoAsiste { get; set; }
        public String ProgramaInteres { get; set; }
        public String Detalle { get; set; }
        public String QueEstudiar { get; set; }
        public String InformacionFaltante { get; set; }

    }
}
