using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO.VO
{
    public class GestorVO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string DAS { get; set; }
        public string SenhaHash { get; set; }
        public bool Ativo { get; set; } = true;

    }
}
