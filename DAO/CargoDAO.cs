using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public class CargoDAO
    {
        private readonly db_apontamento objBanco;

        public CargoDAO()
        {
            objBanco = new db_apontamento();
        }

        
        public List<tb_cargo> Listar()
        {
            //Lista os cargos cadastrado no BD, usando um filtro para ocultar o cargo de gestor
            return objBanco.tb_cargo.Where(c => c.id_cargo != 4).OrderBy(c => c.descricao).ToList();
        }
    }

}
