using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO.VO
{
    public class ColaboradorVO
    {
        public int IdColaborador { get; set; }

        public string Nome { get; set; }

        
        public string Operacao { get; set; }

        //Horários como TimeSpan (hora do dia)
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraAlmocoInicio { get; set; }
        public TimeSpan? HoraAlmocoFim { get; set; }
        public TimeSpan? HoraSaida { get; set; }

        //Day offs armazenados como texto (ex: "SAT", "SUN" ou nomes)
        public DayOfWeek DayOff1 { get; set; }

        public DayOfWeek DayOff2 { get; set; }

        public int DomingoOff { get; set; }

        public bool Ativo { get; set; } = true;

        public int IdGestor { get; set; }
        public int IdJornadaTrabalho { get; set; }
        public decimal valor_hora { get; set; }

    }
}
