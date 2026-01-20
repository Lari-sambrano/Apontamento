using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO.VO
{
    public class EscalaExcelVO
    {
        public string Analista { get; set; }

        // Dia -> (Entrada, Saída)
        public Dictionary<int, (TimeSpan? Inicio, TimeSpan? Fim)> Dias { get; set; }

        public EscalaExcelVO()
        {
            Dias = new Dictionary<int, (TimeSpan?, TimeSpan?)>();
        }
    }
}
