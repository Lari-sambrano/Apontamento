using System;
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

        public void Atualizar(tb_escala escala)
        {
            using (var ctx = new db_apontamento())
            {
                var atual = ctx.tb_escala.FirstOrDefault(x => x.id_escala == escala.id_escala);
                if (atual == null) throw new Exception("Escala não encontrada.");

                atual.descricao = escala.descricao;
                atual.hora_entrada = escala.hora_entrada;
                atual.hora_almoco_inicio = escala.hora_almoco_inicio;
                atual.hora_almoco_fim = escala.hora_almoco_fim;
                atual.hora_saida = escala.hora_saida;
                atual.tipo_escala = escala.tipo_escala;
                atual.dayoff_1 = escala.dayoff_1;
                atual.dayoff_2 = escala.dayoff_2;
                atual.domingo_off = escala.domingo_off;

                // se fizer sentido: atualizar id_gestor ou manter original
                atual.id_gestor = escala.id_gestor;

                ctx.SaveChanges();
            }
        }


        public List<tb_escala> ListarSemImportadas()
        {
            return objBanco.tb_escala
                .Where(e => e.ativo == true)
                .Where(e => e.descricao == null || !e.descricao.StartsWith("Escala importada"))
                .OrderBy(e => e.hora_entrada)
                .ToList();
        }

    }

}
