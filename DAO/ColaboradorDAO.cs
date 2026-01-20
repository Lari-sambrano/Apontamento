using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace DAO
{
    public class ColaboradorDAO
    {
        private readonly db_apontamento objBanco;

        public ColaboradorDAO()
        {
            objBanco = new db_apontamento();
        }


        // CREATE
        public void Cadastrar(tb_colaborador obj)
        {
            objBanco.tb_colaborador.Add(obj);
            objBanco.SaveChanges();
        }

        //READ (LISTAR)
        public List<tb_colaborador> Consultar(int idGestor)
        {
            //Lista todos os colaboradores ativos de um gestor, incluindo dados relacionados
            return objBanco.tb_colaborador
                .Include(c => c.tb_operacao)
                .Include(c => c.tb_cargo)
                .Include(c => c.tb_escala)
                .Include(c => c.tb_gcm)
                .Where(c => c.id_gestor == idGestor && c.status_colaborador == true)
                .OrderBy(c => c.nome_colaborador).ToList();
        }

        //READ (POR ID)

        public tb_colaborador BuscarPorId(int idColaborador)
        {
            //Busca um colaborador específico por ID, incluindo dados relacionados
            return objBanco.tb_colaborador
                .Include(c => c.tb_operacao)
                .Include(c => c.tb_escala).FirstOrDefault(c => c.id_colaborador == idColaborador);
        }

        //UPDATE/ALTERAR
        public void Alterar(tb_colaborador obj)
        {
            //Busca o colaborador existente no banco de dados
            tb_colaborador objResgate = objBanco.tb_colaborador.First(c => c.id_colaborador == obj.id_colaborador);

            //Atualiza os campos do colaborador com os novos valores
            objResgate.nome_colaborador = obj.nome_colaborador;
            objResgate.id_operacao = obj.id_operacao;
            objResgate.id_cargo = obj.id_cargo;
            objResgate.id_escala = obj.id_escala;
            objResgate.id_gcm = obj.id_gcm;
            objResgate.status_colaborador = obj.status_colaborador;
            //Salva as alterações no banco de dados
            objBanco.SaveChanges();
        }

        //DELETE LÓGICO (INATIVAR)
        public void Inativar(int idColaborador)
        {
            //Busca o colaborador existente no banco de dados
            tb_colaborador objResgate = objBanco.tb_colaborador.First(c => c.id_colaborador == idColaborador);
            //Define o status do colaborador como inativo (false)
            objResgate.status_colaborador = false;
            //Salva as alterações no banco de dados
            objBanco.SaveChanges();
        }

        //Listar Colaboradores Ativos
        public List<tb_colaborador> ListarAtivos(int idGestor)
        {
            //Lista todos os colaboradores ativos de um gestor específico
            return objBanco.tb_colaborador.Where(c => c.status_colaborador == true && c.id_gestor == idGestor)
                                        .OrderBy(c => c.nome_colaborador).ToList();
        }

    }
}
