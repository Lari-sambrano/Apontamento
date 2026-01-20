using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.VO
{
    public class TimeSheetMensalLinha
    {
        public int IdColaborador { get; set; }
        public string Nome { get; set; }
        public Dictionary<int, TimeSheetDiaVO> Dias { get; set; }
        public decimal TotalMes { get; set; }
    }
}
