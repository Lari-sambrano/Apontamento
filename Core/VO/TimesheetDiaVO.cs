using System;

namespace Core.VO
{
    public class TimeSheetDiaVO
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
        public decimal HorasAtestado { get; set; }
        public bool IsFeriado { get; set; }
        public decimal AdicionalFeriado { get; set; }
    }
}
