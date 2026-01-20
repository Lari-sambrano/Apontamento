using System.Collections.Generic;
using System.Linq;

namespace DAO
{
    public class OperacaoDAO
    {
        private readonly db_apontamento objBanco = new db_apontamento();

        public List<tb_operacao> ListarAtivas()
        {
            //Retorna todas as operações ativas presentes no banco de dados
            return objBanco.tb_operacao.Where(o => o.ativo).OrderBy(o => o.nome_operacao).ToList();
        }

        //Para uma futura funcionalidade de cadastro de operações
        public void Inserir(tb_operacao operacao)
        {
            //Adiciona uma nova operação ao banco de dados
            objBanco.tb_operacao.Add(operacao);
            //Salva as alterações no banco de dados
            objBanco.SaveChanges();
        }
    }
}
