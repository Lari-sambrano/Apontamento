using System.Collections.Generic;
using System.Linq;

namespace DAO
{
    public class EscalaDAO
    {
        private readonly db_apontamento objBanco = new db_apontamento();

        public void Inserir(tb_escala escala)
        {
            //Adiciona o objeto 'escala' ao tb_escala
            objBanco.tb_escala.Add(escala);
            //Salva as alterações/adições no banco de dados
            objBanco.SaveChanges();
        }

        public List<tb_escala> Listar()
        {
            return objBanco.tb_escala.Where(e => e.ativo == true).OrderBy(e => e.hora_entrada).ToList();
        }

        /// <summary>
        /// Implementação FUTURA para buscar escala por ID
        /// </summary>
        /// <param name="idEscala"></param>
        /// <returns></returns>
        public tb_escala BuscarPorId(int idEscala)
        {
            //Retorna a primeira escala encontrada com o id informado ou null caso não exista
            return objBanco.tb_escala.FirstOrDefault(e => e.id_escala == idEscala);
        }
        /// <summary>
        /// implementação FUTURA para buscar escala por colaborador
        /// </summary>
        /// <param name="idColaborador"></param>
        /// <returns></returns>
        public tb_escala ObterPorColaborador(int idColaborador)
        {
            //Busca o colaborador pelo ID e retorna a escala associada a ele (tb_escala) ou null se não existir
            return objBanco.tb_colaborador.Where(c => c.id_colaborador == idColaborador)
                                         .Select(c => c.tb_escala).FirstOrDefault();
        }


    }

}
