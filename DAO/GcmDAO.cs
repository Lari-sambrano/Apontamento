using DAO.VO;
using System.Collections.Generic;
using System.Linq;

namespace DAO
{
    public class GcmDAO
    {
        private readonly db_apontamento objBanco;

        public GcmDAO()
        {
            objBanco = new db_apontamento();
        }

        public List<tb_gcm> Listar()
        {
            //Retorna a lista de GCMs
            return objBanco.tb_gcm.OrderBy(g => g.descricao).ToList();
        }
        //Buscar GCM por ID
        public tb_gcm BuscarPorId(int idGcm)
        {
            return objBanco.tb_gcm.FirstOrDefault(g => g.id_gcm == idGcm);
        }

        //Atualizar valor hora da GCM
        public void AtualizarValorHora(int idGcm, decimal valorHora)
        {
            var gcm = objBanco.tb_gcm.FirstOrDefault(g => g.id_gcm == idGcm);

            if (gcm != null)
            {
                gcm.valor_hora = valorHora;
                objBanco.SaveChanges();
            }
        }
        public List<GcmVO> ListarParaCombo()
        {
            return objBanco.tb_gcm.Select(g => new GcmVO
            {
                id_gcm = g.id_gcm,
                descricao = g.descricao,
                valor_hora = g.valor_hora
            }).OrderBy(g => g.descricao).ToList();
        }


    }

}
