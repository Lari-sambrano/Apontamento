using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.VO
{
    public class TimeSheetVO
    {
        public DateTime Data { get; set; }
        public string DiaSemana { get; set; }

        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraAlmocoInicio { get; set; }
        public TimeSpan? HoraAlmocoFim { get; set; }
        public TimeSpan? HoraSaida { get; set; }

        public string Codigo { get; set; }

        public decimal HorasTrabalhadas { get; set; }
        public decimal BancoHoras { get; set; }
        public decimal ValorDia { get; set; }

        public bool PossuiExcecao { get; set; }
    }
}
