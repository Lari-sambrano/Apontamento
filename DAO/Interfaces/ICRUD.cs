using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO.Interfaces
{
    public interface ICRUD<T> where T : class
    {
        void Cadastrar(T obj);
        void Alterar(T obj);
        List<T> Consultar(int codLogado);
        void Excluir(int id);
    }
}
