using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAO.VO
{
    public class AnalistaResumoVO
    {

        public string Nome { get; set; }

        // Dia -> Tipo (1 a 31)
        public Dictionary<int, WorkType> Dias { get; set; }
            = new Dictionary<int, WorkType>();

        public Dictionary<WorkType, int> Totais { get; set; }
            = Enum.GetValues(typeof(WorkType))
                  .Cast<WorkType>()
                  .ToDictionary(x => x, x => 0);

    }
    
}
